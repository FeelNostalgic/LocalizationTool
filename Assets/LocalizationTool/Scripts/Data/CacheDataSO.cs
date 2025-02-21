using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LocalizationTool.Scripts.Data
{
    public class CacheDataSO
    {
        #region PUBLIC VARIABLES

        public static CacheDataSO Instance => _instance ??= new CacheDataSO();
        public static bool IsDataLoaded { get; private set; }

        public static List<LanguageDataSO> LanguageCache { get; private set; }
        public static List<string> Languages => LanguageCache.Select(x => x.languageName).ToList();
        public static string DefaultLanguage => LanguageCache.FirstOrDefault(x => x.isDefault)?.languageName;

        public static List<CategoryDataSO> CategoryCache { get; private set; }
        public static List<string> Categories => CategoryCache.Select(x => x.categoryName).ToList();
        public static string DefaultCategory => CategoryCache.FirstOrDefault(x => x.isDefault)?.categoryName;

        public Action OnLocalizationToolDataInitialized { get; set; }

        #endregion

        #region PRIVATE VARIABLES

        private static CacheDataSO _instance;
        private static LocalizationDataSO _localizationData;

        private const string FOLDER_PATH = "Assets/LocalizationTool/Database";
        private const string LANGUAGES_PATH = FOLDER_PATH + "/Languages";
        private const string CATEGORIES_PATH = FOLDER_PATH + "/Categories";
        private const string KEYS_PATH = FOLDER_PATH + "/Keys";
        private const string DATABASE_PATH = FOLDER_PATH + "/LocalizationData.asset";

        #endregion

        #region LOAD CACHE

        public void InitForEditor(Action onComplete = null)
        {
            if (IsDataLoaded) return;

            CreateLocationData();
            LoadCacheData();
            IsDataLoaded = true;
            onComplete?.Invoke();
        }

        private static void LoadCacheData()
        {
            UpdateLanguageCache();

            UpdateCategoriesCache();

#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
            
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;
            
            Instance.OnLocalizationToolDataInitialized?.Invoke();
        }

        public static void LoadLanguagesCache()
        {
            CreateLocationData();
            UpdateLanguageCache();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;
        }

        public static void LoadCategoriesCache()
        {
            CreateLocationData();
            UpdateCategoriesCache();
        }

        #endregion

        #region KEYS

        public static void InsertKey(string keyName, string categoryName)
        {
            foreach (var languageData in LanguageCache)
            {
                var translation = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
                translation.name = keyName;
                translation.keyName = keyName;
                translation.category = CategoryCache.FirstOrDefault(x => x.categoryName.Equals(categoryName));
                translation.displayOrder = languageData.translationKeys.Count + 1;
                languageData.translationKeys.Add(translation);
                AssetDatabase.AddObjectToAsset(translation, languageData);
            }

            SaveChanges();
        }

        #endregion

        #region LANGUAGES

        public static void InsertLanguage(string newLanguageName)
        {
            var newLanguage = ScriptableObject.CreateInstance<LanguageDataSO>();
            AssetDatabase.CreateAsset(newLanguage, $"{LANGUAGES_PATH}/{newLanguageName}.asset");
            newLanguage.languageName = newLanguageName;
            newLanguage.displayOrder = _localizationData.languages.Count + 1;
            if (_localizationData.languages.Count == 1) newLanguage.isDefault = true;

            _localizationData.languages.Add(newLanguage);
            SaveChanges();
            UpdateLanguageCache();
        }

        public static void SetDefaultLanguage(string newDefaultLanguage)
        {
            // Remove old default
            var currentDefault = _localizationData.languages.FirstOrDefault(l => l.isDefault);
            System.Diagnostics.Debug.Assert(currentDefault, nameof(currentDefault) + " != null");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = _localizationData.languages.FirstOrDefault(l => l.languageName == newDefaultLanguage);
            System.Diagnostics.Debug.Assert(newDefault, nameof(newDefault) + " != null");
            newDefault.isDefault = true;

            // Reorder languages
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var language in _localizationData.languages.Where(language => language.displayOrder < newDefaultOrder))
            {
                language.displayOrder++; // Shift languages above the new default
            }

            newDefault.displayOrder = 1;

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void RemoveLanguage(string languageName)
        {
            var languageToRemove = _localizationData.languages.FirstOrDefault(c => c.languageName.Equals(languageName));
            var removedDisplayOrder = languageToRemove!.displayOrder;

            // Update display order
            foreach (var language in _localizationData.languages.Where(language => language.displayOrder > removedDisplayOrder))
            {
                language.displayOrder--;
            }

            _localizationData.languages.Remove(languageToRemove);

            RemoveAsset(languageToRemove);

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void UpdateLanguageDisplayOrder(string languageName, int oldDisplayOrder, int newDisplayOrder)
        {
            // Find the category to update
            var languageToUpdate = _localizationData.languages.FirstOrDefault(l => l.languageName.Equals(languageName));

            // Reorder categories
            if (oldDisplayOrder < newDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var language in _localizationData.languages.Where(language => language.displayOrder > oldDisplayOrder && language.displayOrder <= newDisplayOrder))
                {
                    language.displayOrder--; // Shift down
                }
            }
            else if (oldDisplayOrder > newDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var language in _localizationData.languages.Where(language => language.displayOrder >= newDisplayOrder && language.displayOrder < oldDisplayOrder))
                {
                    language.displayOrder++; // Shift up
                }
            }

            // Update the target's display order
            languageToUpdate!.displayOrder = newDisplayOrder;

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void UpdateLanguageName(string oldLanguageName, string newLanguageName)
        {
            var languageToUpdate = _localizationData.languages.FirstOrDefault(l => l.languageName == oldLanguageName);
            AssetDatabase.RenameAsset(GetPath(languageToUpdate), newLanguageName);
            languageToUpdate!.languageName = newLanguageName;

            SaveChanges();
        }

        #endregion

        #region CATEGORIES

        public static void InsertCategory(string newCategoryName)
        {
            var newCategory = ScriptableObject.CreateInstance<CategoryDataSO>();
            AssetDatabase.CreateAsset(newCategory, $"{CATEGORIES_PATH}/{newCategoryName}.asset");
            newCategory.categoryName = newCategoryName;
            newCategory.displayOrder = _localizationData.categories.Count + 1;
            if (_localizationData.categories.Count == 1) newCategory.isDefault = true;

            _localizationData.categories.Add(newCategory);
            SaveChanges(newCategory);
            UpdateCategoriesCache();
        }

        public static void SetDefaultCategory(string newDefaultCategory)
        {
            // Remove old default
            var currentDefault = _localizationData.categories.FirstOrDefault(c => c.isDefault);
            System.Diagnostics.Debug.Assert(currentDefault, nameof(currentDefault) + " != null");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = _localizationData.categories.FirstOrDefault(c => c.categoryName == newDefaultCategory);
            System.Diagnostics.Debug.Assert(newDefault, nameof(newDefault) + " != null");
            newDefault.isDefault = true;

            // Reorder categories
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var category in _localizationData.categories.Where(category => category.displayOrder < newDefaultOrder))
            {
                category.displayOrder++; // Shift categories above the new default
            }

            newDefault.displayOrder = 1;

            UpdateCategoriesCache();
            SaveChanges();
        }

        public static void RemoveCategory(string categoryName)
        {
            var categoryToRemove = _localizationData.categories.FirstOrDefault(c => c.categoryName.Equals(categoryName));
            var removedDisplayOrder = categoryToRemove!.displayOrder;

            // Update keys that use the categoryToRemove to default category
            foreach (var translationKey in _localizationData.languages.SelectMany(localizationDataLanguage => localizationDataLanguage.translationKeys.Where(x => x.category.categoryName.Equals(categoryName))))
            {
                translationKey.category.categoryName = DefaultCategory;
            }

            // Update display order
            foreach (var category in _localizationData.categories.Where(category => category.displayOrder > removedDisplayOrder))
            {
                category.displayOrder--;
            }

            _localizationData.categories.Remove(categoryToRemove);

            RemoveAsset(categoryToRemove);

            UpdateCategoriesCache();
            SaveChanges();
        }

        public static void UpdateCategoryDisplayOrder(string categoryName, int oldCategoryDisplayOrder, int newCategoryDisplayOrder)
        {
            // Find the category to update
            var categoryToUpdate = _localizationData.categories.FirstOrDefault(c => c.categoryName == categoryName);

            // Reorder categories
            if (oldCategoryDisplayOrder < newCategoryDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var category in _localizationData.categories.Where(category => category.displayOrder > oldCategoryDisplayOrder && category.displayOrder <= newCategoryDisplayOrder))
                {
                    category.displayOrder--; // Shift categories down
                }
            }
            else if (oldCategoryDisplayOrder > newCategoryDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var category in _localizationData.categories.Where(category => category.displayOrder >= newCategoryDisplayOrder && category.displayOrder < oldCategoryDisplayOrder))
                {
                    category.displayOrder++; // Shift categories up
                }
            }

            // Update the target category's display order
            categoryToUpdate!.displayOrder = newCategoryDisplayOrder;

            UpdateCategoriesCache();
            SaveChanges();
        }

        public static void UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            var categoryToUpdate = _localizationData.categories.FirstOrDefault(c => c.categoryName == oldCategoryName);
            AssetDatabase.RenameAsset(GetPath(categoryToUpdate), newCategoryName);
            categoryToUpdate!.categoryName = newCategoryName;

            SaveChanges();
        }

        #endregion

        #region PRIVATE METHODS

        private static void CreateLocationData()
        {
            _localizationData = AssetDatabase.LoadAssetAtPath<LocalizationDataSO>(DATABASE_PATH);

            if (_localizationData) return;
            _localizationData = ScriptableObject.CreateInstance<LocalizationDataSO>();
            AssetDatabase.CreateAsset(_localizationData, DATABASE_PATH);
            SaveChanges();
            Debug.Log("LocalizationDataSO.asset created!");

            InsertCategory("None");
            InsertLanguage("English");
        }

        private static void SaveChanges(Object obj = null)
        {
            EditorUtility.SetDirty(_localizationData);
            if (obj.IsNotNull()) EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RemoveAsset(Object assetToRemove)
        {
            var assetPath = GetPath(assetToRemove);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static string GetPath(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset);
        }
        
        private static void UpdateLanguageCache()
        {
            LanguageCache = _localizationData.languages
                .OrderBy(l => l.displayOrder)
                .ToList();
        }

        private static void UpdateCategoriesCache()
        {
            CategoryCache = _localizationData.categories
                .OrderBy(c => c.displayOrder)
                .ToList();
        }

        #endregion
    }
}
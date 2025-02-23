using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.Editors;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LocalizationTool.Scripts.Data
{
    public static class CacheDataSO
    {
        #region PUBLIC VARIABLES

        //public static CacheDataSO Instance => _instance ??= new CacheDataSO();
        public static bool IsDataLoaded { get; private set; }

        public static List<KeyDataSO> Keys => localizationData.keys;

        public static List<LanguageDataSO> LanguageCache { get; private set; }
        public static List<string> Languages => LanguageCache.Select(x => x.languageName).ToList();
        public static string DefaultLanguage => LanguageCache.FirstOrDefault(x => x.isDefault)?.languageName;

        public static List<CategoryDataSO> CategoryCache { get; private set; }
        public static List<string> Categories => CategoryCache.Select(x => x.categoryName).ToList();
        public static string DefaultCategory => CategoryCache.FirstOrDefault(x => x.isDefault)?.categoryName;

        public static Action OnLocalizationToolDataInitialized { get; set; }

        #endregion

        #region PRIVATE VARIABLES

        //private static CacheDataSO _instance;
        public static LocalizationDataSO localizationData;

        private const string DATABASE_PATH = "Assets/LocalizationTool/Database";
        private const string LANGUAGES_PATH = DATABASE_PATH + "/Languages";
        private const string CATEGORIES_PATH = DATABASE_PATH + "/Categories";
        private const string KEYS_PATH = DATABASE_PATH + "/Keys";
        private const string LOCALIZATION_DATA_PATH = DATABASE_PATH + "/LocalizationData.asset";
        
        // Undo Tracker
        private static Stack<(string, string)> _undoRenameCategoryTracker = new();
        private static Stack<(string, string)> _undoRenameLanguageTracker = new();

        #endregion

        #region LOAD CACHE

        public static void InitForEditor(Action onComplete = null)
        {
            if (IsDataLoaded) return;

            LoadCacheData();
            IsDataLoaded = true;
            onComplete?.Invoke();
        }

        public static void LoadCacheData()
        {
            if (IsDataLoaded) return;

            CreateOrLoadLocalizationData();

            UpdateLanguageCache();

            UpdateCategoriesCache();

#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif

            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;

            IsDataLoaded = true;

            OnLocalizationToolDataInitialized?.Invoke();
        }

        public static void LoadLanguagesCache()
        {
            CreateOrLoadLocalizationData();
            UpdateLanguageCache();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;
        }

        public static void LoadCategoriesCache()
        {
            CreateOrLoadLocalizationData();
            UpdateCategoriesCache();
        }

        public static void LoadKeysCache()
        {
            CreateOrLoadLocalizationData();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
        }

        public static void ClearData()
        {
            var tempKList = new List<KeyDataSO>(Keys);
            tempKList.ForEach(x => RemoveKey(x.keyName));

            var tempLList = new List<LanguageDataSO>(LanguageCache);
            tempLList.ForEach(x => RemoveLanguage(x.languageName));

            var tempCList = new List<CategoryDataSO>(CategoryCache);
            tempCList.ForEach(x => RemoveCategory(x.categoryName));
        }

        #endregion

        #region KEYS

        public static void InsertKey(string keyName, string categoryName)
        {
            CreateDirectoryIfNotExist(KEYS_PATH);

            var newKeyData = ScriptableObject.CreateInstance<KeyDataSO>();
            AssetDatabase.CreateAsset(newKeyData, $"{KEYS_PATH}/{keyName}.asset");
            newKeyData.keyName = keyName;
            newKeyData.displayOrder = localizationData.keys.Count + 1;
            newKeyData.category = CategoryCache.FirstOrDefault(x => x.categoryName.Equals(categoryName));
            localizationData.keys.Add(newKeyData);

            foreach (var languageData in LanguageCache)
            {
                var translation = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
                translation.name = keyName;
                translation.keyData = newKeyData;
                languageData.translationKeys.Add(translation);
                AssetDatabase.AddObjectToAsset(translation, languageData);
            }

            SaveChanges();
        }

        public static void RemoveKey(string keyName)
        {
            var keyToRemove = localizationData.KeysDictionary[keyName];
            localizationData.keys.Remove(keyToRemove);
            RemoveAsset(keyToRemove);

            foreach (var localizationDataLanguage in localizationData.languages)
            {
                var translationToRemove = localizationDataLanguage.TranslationDictionary[keyName];
                localizationDataLanguage.translationKeys.Remove(translationToRemove);
                AssetDatabase.RemoveObjectFromAsset(translationToRemove);
                Object.DestroyImmediate(translationToRemove, true);
            }

            SaveChanges();
        }

        public static void UpdateKeyCategory(string key, string newCategory)
        {
            var keyToUpdate = localizationData.KeysDictionary[key];
            Undo.RecordObject(keyToUpdate, $"Key {key} updated category to {newCategory}");
            keyToUpdate.category = localizationData.CategoriesDictionary[newCategory];

            SaveChanges();
        }

        public static void SetKeyTranslation(string key, string newTranslation, string language)
        {
            var translationToUpdate = localizationData.LanguagesDictionary[language].TranslationDictionary[key];
            Undo.RecordObject(translationToUpdate, $"Changed translation {key} in {language} to {newTranslation}");
            translationToUpdate.translationText = newTranslation;
            
            SaveChanges();
            
            Undo.undoRedoPerformed -= DictionaryEditor.RefreshTextBuffer;
            Undo.undoRedoPerformed -= RichTextEditor.RefreshTextArea;
            Undo.undoRedoPerformed += DictionaryEditor.RefreshTextBuffer;
            Undo.undoRedoPerformed += RichTextEditor.RefreshTextArea;
        }

        public static void UpdateKeyName(string oldKeyName, string newKeyName)
        {
            var keyToUpdate = localizationData.keys.FirstOrDefault(x => x.keyName.Equals(oldKeyName));
            // FUTURE: Undo.RecordObject(keyToUpdate, $"Rename Key '{oldKeyName}' to '{newKeyName}'");
            keyToUpdate!.keyName = newKeyName;
            EditorUtility.SetDirty(keyToUpdate);
            AssetDatabase.RenameAsset(GetPath(keyToUpdate), newKeyName);
            
            foreach (var language in LanguageCache)
            {
                if (!language.TranslationDictionary.TryGetValue(oldKeyName, out var translationToUpdate)) continue;

                // FUTURE: Undo.RecordObject(translationToUpdate, $"Rename Translation Key '{oldKeyName}' to '{newKeyName}'");
                translationToUpdate.name = newKeyName;
                EditorUtility.SetDirty(translationToUpdate);
            }

            SaveChanges();
        }

        #endregion

        #region LANGUAGES

        public static void InsertLanguage(string newLanguageName)
        {
            var newLanguage = ScriptableObject.CreateInstance<LanguageDataSO>();
            newLanguage.languageName = newLanguageName;
            newLanguage.displayOrder = localizationData.languages.Count + 1;
            if (localizationData.languages.Count == 0) newLanguage.isDefault = true;
            AssetDatabase.CreateAsset(newLanguage, $"{LANGUAGES_PATH}/{newLanguageName}.asset");

            localizationData.languages.Add(newLanguage);

            foreach (var keyData in localizationData.keys)
            {
                var translation = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
                translation.name = keyData.keyName;
                translation.keyData = keyData;
                newLanguage.translationKeys.Add(translation);
                AssetDatabase.AddObjectToAsset(translation, newLanguage);
            }

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void SetDefaultLanguage(string newDefaultLanguage)
        {
            // Remove old default
            var currentDefault = LanguageCache[0];
            System.Diagnostics.Debug.Assert(currentDefault, nameof(currentDefault) + " != null");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = localizationData.LanguagesDictionary[newDefaultLanguage];
            System.Diagnostics.Debug.Assert(newDefault, nameof(newDefault) + " != null");
            newDefault.isDefault = true;

            // Reorder languages
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var language in localizationData.languages.Where(language => language.displayOrder < newDefaultOrder))
            {
                language.displayOrder++; // Shift languages above the new default
            }

            newDefault.displayOrder = 1;

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void RemoveLanguage(string languageNameToRemove)
        {
            var languageToRemove = localizationData.LanguagesDictionary[languageNameToRemove];
            var removedDisplayOrder = languageToRemove!.displayOrder;

            // Update display order
            foreach (var language in localizationData.languages.Where(language => language.displayOrder > removedDisplayOrder))
            {
                language.displayOrder--;
            }

            localizationData.languages.Remove(languageToRemove);

            foreach (var translationKeyData in languageToRemove.translationKeys)
            {
                AssetDatabase.RemoveObjectFromAsset(translationKeyData);
                Object.DestroyImmediate(translationKeyData, true);
            }

            RemoveAsset(languageToRemove);

            UpdateLanguageCache();
            SaveChanges();

            if (LocalizationManager.CurrentLanguageInDictionarySection.Equals(languageNameToRemove))
            {
                LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
            }
        }

        public static void UpdateLanguageDisplayOrder(string languageName, int oldDisplayOrder, int newDisplayOrder)
        {
            // Find the category to update
            var languageToUpdate = localizationData.LanguagesDictionary[languageName];
            
            Undo.RecordObject(languageToUpdate, "Changed language display order");

            // Reorder categories
            if (oldDisplayOrder < newDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var language in localizationData.languages.Where(language => language.displayOrder > oldDisplayOrder && language.displayOrder <= newDisplayOrder))
                {
                    Undo.RecordObject(language, "Changed language display order");
                    language.displayOrder--; // Shift down
                }
            }
            else if (oldDisplayOrder > newDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var language in localizationData.languages.Where(language => language.displayOrder >= newDisplayOrder && language.displayOrder < oldDisplayOrder))
                {
                    Undo.RecordObject(language, "Changed language display order");
                    language.displayOrder++; // Shift up
                }
            }
            
            // Update the target's display order
            languageToUpdate!.displayOrder = newDisplayOrder;

            UpdateLanguageCache();
            SaveChanges();

            Undo.undoRedoPerformed -= UpdateLanguagesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateLanguagesCacheAndRepaint;
        }

        private static void UpdateLanguagesCacheAndRepaint()
        {
            UpdateLanguageCache();
            LocalizationMainEditor.Instance.Repaint();
            SaveChanges();
        }

        public static void UpdateLanguageName(string oldLanguageName, string newLanguageName)
        {
            var languageToUpdate = localizationData.LanguagesDictionary[oldLanguageName];
            
            Undo.RecordObject(languageToUpdate, $"Change Language {oldLanguageName} Name to {newLanguageName}");
            _undoRenameLanguageTracker.Push((oldLanguageName, newLanguageName));
            
            languageToUpdate!.languageName = newLanguageName;
            EditorUtility.SetDirty(languageToUpdate);
            AssetDatabase.RenameAsset(GetPath(languageToUpdate), newLanguageName);

            SaveChanges();
            
            Undo.undoRedoPerformed -= HandleUpdateLanguageNameUndoRedo;
            Undo.undoRedoPerformed += HandleUpdateLanguageNameUndoRedo;
        }
        
        private static void HandleUpdateLanguageNameUndoRedo()
        {
            if (_undoRenameLanguageTracker.IsEmpty()) return;
            var previousCurrentName = _undoRenameLanguageTracker.Pop();
            
            var assetPath = $"{LANGUAGES_PATH}/{previousCurrentName.Item2}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath).IsNull()) return;
            AssetDatabase.RenameAsset(assetPath, previousCurrentName.Item1);
            
            SaveChanges();
        }

        public static void EmptyLanguage(string languageToEmpty)
        {
            var languageToUpdate = localizationData.LanguagesDictionary[languageToEmpty];
            foreach (var translationKeyData in languageToUpdate.translationKeys)
            {
                translationKeyData.translationText = "";
            }

            SaveChanges();
        }

        #endregion

        #region CATEGORIES

        public static void InsertCategory(string newCategoryName)
        {
            var newCategory = ScriptableObject.CreateInstance<CategoryDataSO>();
            newCategory.categoryName = newCategoryName;
            newCategory.displayOrder = localizationData.categories.Count + 1;
            if (localizationData.categories.Count == 0) newCategory.isDefault = true;
            AssetDatabase.CreateAsset(newCategory, $"{CATEGORIES_PATH}/{newCategoryName}.asset");

            localizationData.categories.Add(newCategory);
            SaveChanges(newCategory);
            UpdateCategoriesCache();
        }

        public static void SetDefaultCategory(string newDefaultCategory)
        {
            // Remove old default
            var currentDefault = CategoryCache[0];
            Debug.Assert(currentDefault, nameof(currentDefault) + " != null");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = localizationData.CategoriesDictionary[newDefaultCategory];
            Debug.Assert(newDefault, nameof(newDefault) + " != null");
            newDefault.isDefault = true;

            // Reorder categories
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var category in localizationData.categories.Where(category => category.displayOrder < newDefaultOrder))
            {
                category.displayOrder++; // Shift categories above the new default
            }

            newDefault.displayOrder = 1;

            UpdateCategoriesCache();
            SaveChanges();
        }

        public static void RemoveCategory(string categoryName)
        {
            var categoryToRemove = localizationData.CategoriesDictionary[categoryName];
            var removedDisplayOrder = categoryToRemove!.displayOrder;

            // Update keys that use the categoryToRemove to default category
            foreach (var key in localizationData.keys.Where(x => x.category.categoryName.Equals(categoryName)))
            {
                key.category = localizationData.CategoriesDictionary[DefaultCategory];
            }

            // Update display order
            foreach (var category in localizationData.categories.Where(category => category.displayOrder > removedDisplayOrder))
            {
                category.displayOrder--;
            }

            localizationData.categories.Remove(categoryToRemove);

            RemoveAsset(categoryToRemove);

            UpdateCategoriesCache();
            SaveChanges();
        }

        public static void UpdateCategoryDisplayOrder(string categoryName, int oldCategoryDisplayOrder, int newCategoryDisplayOrder)
        {
            // Find the category to update
            var categoryToUpdate = localizationData.CategoriesDictionary[categoryName];

            Undo.RecordObject(categoryToUpdate, "Changed category display order");
            
            // Reorder categories
            if (oldCategoryDisplayOrder < newCategoryDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var category in localizationData.categories.Where(category => category.displayOrder > oldCategoryDisplayOrder && category.displayOrder <= newCategoryDisplayOrder))
                {
                    Undo.RecordObject(category, "Changed category display order");
                    category.displayOrder--; // Shift categories down
                }
            }
            else if (oldCategoryDisplayOrder > newCategoryDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var category in localizationData.categories.Where(category => category.displayOrder >= newCategoryDisplayOrder && category.displayOrder < oldCategoryDisplayOrder))
                {
                    Undo.RecordObject(category, "Changed category display order");
                    category.displayOrder++; // Shift categories up
                }
            }

            // Update the target category's display order
            categoryToUpdate!.displayOrder = newCategoryDisplayOrder;

            UpdateCategoriesCache();
            SaveChanges();

            Undo.undoRedoPerformed -= UpdateCategoriesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateCategoriesCacheAndRepaint;
        }

        private static void UpdateCategoriesCacheAndRepaint()
        {
            UpdateCategoriesCache();
            LocalizationMainEditor.Instance.Repaint();
            SaveChanges();
        }
        
        public static void UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            var categoryToUpdate = localizationData.CategoriesDictionary[oldCategoryName];
            
            Undo.RecordObject(categoryToUpdate, $"Change Category {oldCategoryName} Name to {newCategoryName}");
            _undoRenameCategoryTracker.Push((oldCategoryName, newCategoryName));
            
            categoryToUpdate!.categoryName = newCategoryName;
            EditorUtility.SetDirty(categoryToUpdate);
            AssetDatabase.RenameAsset(GetPath(categoryToUpdate), newCategoryName);

            SaveChanges();
            
            Undo.undoRedoPerformed -= HandleUpdateCategoryNameUndoRedo;
            Undo.undoRedoPerformed += HandleUpdateCategoryNameUndoRedo;
        }
        
        private static void HandleUpdateCategoryNameUndoRedo()
        {
            if (_undoRenameCategoryTracker.IsEmpty()) return;
            var previousCurrentName = _undoRenameCategoryTracker.Pop();
            
            var assetPath = $"{CATEGORIES_PATH}/{previousCurrentName.Item2}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath).IsNull()) return;
            AssetDatabase.RenameAsset(assetPath, previousCurrentName.Item1);
            
            SaveChanges();
        }

        #endregion

        #region PRIVATE METHODS

        private static void CreateOrLoadLocalizationData()
        {
            CreateDirectoryIfNotExist(DATABASE_PATH);

            localizationData = AssetDatabase.LoadAssetAtPath<LocalizationDataSO>(LOCALIZATION_DATA_PATH) ?? ScriptableObject.CreateInstance<LocalizationDataSO>();

            if (!AssetDatabase.Contains(localizationData))
            {
                AssetDatabase.CreateAsset(localizationData, LOCALIZATION_DATA_PATH);
                //Debug.Log("LocalizationData.asset created!");
            }

            InsertFirstLanguageIfNotExist();
            InsertFirstCategoryIfNotExist();
            SaveChanges();

            //Debug.Log("LocalizationData.asset loaded!");
        }

        private static void InsertFirstLanguageIfNotExist()
        {
            if (localizationData.languages.IsNotEmpty()) return;

            CreateDirectoryIfNotExist(LANGUAGES_PATH);
            InsertLanguage("English");
        }

        private static void InsertFirstCategoryIfNotExist()
        {
            if (localizationData.categories.IsNotEmpty()) return;

            CreateDirectoryIfNotExist(CATEGORIES_PATH);
            InsertCategory("None");
        }

        private static void SaveChanges(Object obj = null)
        {
            EditorUtility.SetDirty(localizationData);
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

        private static void CreateDirectoryIfNotExist(string path)
        {
            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }


        private static string GetPath(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset);
        }

        private static void UpdateLanguageCache()
        {
            LanguageCache = localizationData.languages
                .OrderBy(l => l.displayOrder)
                .ToList();
        }

        private static void UpdateCategoriesCache()
        {
            CategoryCache = localizationData.categories
                .OrderBy(c => c.displayOrder)
                .ToList();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;

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

        public static Dictionary<string, TranslationKeyDataSO> DictionaryCache { get; private set; }

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
            LanguageCache = _localizationData.languages
                .OrderBy(l => l.displayOrder)
                .ToList();

            CategoryCache = _localizationData.categories
                .OrderBy(c => c.displayOrder)
                .ToList();

            DictionaryCache = _localizationData.translationKeys
                .ToDictionary(k => k.keyName, k => k);

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

        #endregion

        #region KEYS

        public static void InsertKey(string keyName, CategoryDataSO category)
        {
            var newKey = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
            newKey.keyName = keyName;
            newKey.category = category;
            newKey.displayOrder = _localizationData.translationKeys.Count + 1;

            _localizationData.translationKeys.Add(newKey);
            AssetDatabase.AddObjectToAsset(newKey, _localizationData);
            SaveChanges();
        }

        #endregion

        #region LANGUAGES

        public static void InsertLanguage(string newLanguageName)
        {
            var newLanguage = ScriptableObject.CreateInstance<LanguageDataSO>();
            AssetDatabase.CreateAsset(newLanguage, $"{LANGUAGES_PATH}/{newLanguageName}.asset");
            //newLanguage.name = newLanguageName;
            newLanguage.languageName = newLanguageName;
            newLanguage.displayOrder = _localizationData.languages.Count + 1;

            _localizationData.languages.Add(newLanguage);
            //AssetDatabase.AddObjectToAsset(newLanguage, _localizationData);
            SaveChanges();
            UpdateLanguageCache();
        }

        #endregion

        #region CATEGORIES

        public static void InsertCategory(string newCategoryName)
        {
            var newCategory = ScriptableObject.CreateInstance<CategoryDataSO>();
            newCategory.categoryName = newCategoryName;
            newCategory.displayOrder = _localizationData.categories.Count + 1;

            _localizationData.categories.Add(newCategory);
            AssetDatabase.AddObjectToAsset(newCategory, _localizationData);
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
        }

        private static void SaveChanges()
        {
            EditorUtility.SetDirty(_localizationData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void UpdateLanguageCache()
        {
            LanguageCache = _localizationData.languages
                .OrderBy(l => l.displayOrder)
                .ToList();
        }

        #endregion
    }
}
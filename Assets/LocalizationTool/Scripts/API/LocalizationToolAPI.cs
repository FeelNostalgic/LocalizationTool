using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.General;
using TMPro;
using UnityEngine;
using Colors = CustomDebugPlugin.Colors;

namespace LocalizationTool.Scripts.API
{
    [AddComponentMenu("Localization Tool/API/Auto translation", 1)]
    [DefaultExecutionOrder(-900)]
    public class LocalizationToolAPI : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public static LocalizationToolAPI Instance => _instance ??= (LocalizationToolAPI) FindObjectOfType(typeof(LocalizationToolAPI));

        public static string ActiveLanguage { get; set; }
        
        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationToolAPI _instance;

        private static List<LocalizationToolAddon> _addons;

        #endregion

        #region UNITY METHODS

        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            FindAllAddons();
            
            CacheDataSO.OnLocalizationToolDataInitialized += delegate
            {
#if UNITY_EDITOR
                LocalizationManager.Log("Localization Tool API", Colors.Pink, "Initialized");
#endif
                UpdateAllAddons(ActiveLanguage);
            };
        }

        private void Start()
        {
            CacheDataSO.LoadCacheData();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Return the value of a key in the current language
        /// </summary>
        /// <param name="key">Key to get value from</param>
        /// <param name="found">True if key was found, otherwise false</param>
        /// <returns>Value of the key. Empty value if key was not found</returns>
        public static string GetValueByKey(string key, out bool found)
        {
            found = false;
            if (!CacheDataSO.IsDataLoaded) return "";
            if (key.IsNull()) return "";

            found = CacheDataSO.localizationData.KeysDictionary.ContainsKey(key);
            if (found) found = CacheDataSO.localizationData.LanguagesDictionary[ActiveLanguage].TranslationDictionary.ContainsKey(key);
            return found
                ? CacheDataSO.localizationData.LanguagesDictionary[ActiveLanguage].TranslationDictionary[key].translationText
                : "";
        }

        /// <summary>
        /// Return the value of a key in the given language
        /// </summary>
        /// <param name="key">Key to get value from</param>
        /// <param name="language">Language to get value from</param>
        /// <param name="found">True if key was found. False if language doesn't exist or key was not found</param>
        /// <returns>The value of the key. Empty value if key was not found</returns>
        public static string GetValueByKeyAndLanguage(string key, string language, out bool found)
        {
            if (!CacheDataSO.Languages.Contains(language))
            {
                found = false;
                return "";
            }

            found = CacheDataSO.localizationData.LanguagesDictionary[language].TranslationDictionary.ContainsKey(key);
            return found
                ? CacheDataSO.localizationData.LanguagesDictionary[language].TranslationDictionary[key].translationText
                : "";
        }

        /// <summary>
        /// Change active language to an available language. All addon will be updated to the new language
        /// </summary>
        /// <param name="newLanguage">An available language</param>
        /// <returns>Return true if newLanguage exist, otherwise return false</returns>
        public static bool ChangeLanguage(string newLanguage)
        {
            if (!CacheDataSO.Languages.Contains(newLanguage)) return false;
            ActiveLanguage = newLanguage;
            UpdateAllAddons(newLanguage);
            return true;
        }

        /// <summary>
        /// Return all available languages, useful for making a language selector
        /// </summary>
        /// <returns>List of all languages</returns>
        public static List<string> GetAvailableLanguages()
        {
            return CacheDataSO.Languages;
        }

        /// <summary>
        /// Return all Categories
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllCategories()
        {
            //CacheData.LoadCacheData();
            return CacheDataSO.Categories;
        }

        /// <summary>
        /// Return all Keys
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
            //CacheData.LoadCacheData();
            if (!CacheDataSO.IsDataLoaded) return null;
            var keys = CacheDataSO.localizationData.keys.Select(x=>x.name).ToList();

            return keys;
        }

        #endregion

        #region PRIVATE METHODS

        private static void UpdateAllAddons(string newLanguage)
        {
            foreach (var addon in _addons)
            {
                addon.LanguageUpdate(newLanguage);
            }
        }

        private static void FindAllAddons()
        {
            var TextMeshPros = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
            _addons = new List<LocalizationToolAddon>();
            foreach (var tmp in TextMeshPros)
            {
                var item = (TextMeshProUGUI)tmp;
                var addon = item.GetComponent<LocalizationToolAddon>();
                if (addon.IsNotNull()) _addons.Add(addon);
            }
        }

        #endregion
    }
}
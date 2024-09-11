using System.Collections.Generic;
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
            
            CacheData.Instance.OnLocalizationToolDataInitialized += delegate
            {
#if UNITY_EDITOR
                LocalizationManager.Log("Localization Tool API", Colors.Pink, "Initialized");
#endif
                UpdateAllAddons(ActiveLanguage);
            };
        }

        private void Start()
        {
            CacheData.LoadCacheData();
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
            if (!CacheData.IsDataLoaded) return "";
            if (key.IsNull()) return "";
            
            found = CacheData.DictionaryCache.ContainsKey(key);
            if(found) found = CacheData.DictionaryCache[key].TranslationData.ContainsKey(ActiveLanguage);
            return found
                ? CacheData.DictionaryCache[key].TranslationData[ActiveLanguage]
                : "";
        }

        /// <summary>
        /// Return the value of a key in the given language
        /// </summary>
        /// <param name="key">Key to get value from</param>
        /// <param name="language">Language to get value from</param>
        /// <param name="found">True if key was found. False if language doesnt exist or key was not found</param>
        /// <returns>The value of the key. Empty value if key was not found</returns>
        public static string GetValueByKeyAndLanguage(string key, string language, out bool found)
        {
            if (!CacheData.Languages.Contains(language))
            {
                found = false;
                return "";
            }

            found = CacheData.DictionaryCache[key].TranslationData.ContainsKey(language);
            return found
                ? CacheData.DictionaryCache[key].TranslationData[ActiveLanguage]
                : "";
        }

        /// <summary>
        /// Change active language to an available language. All addon will be updated to the new language
        /// </summary>
        /// <param name="newLanguage">An available language</param>
        /// <returns>Return true if newLanguage exist, otherwise return false</returns>
        public static bool ChangeLanguage(string newLanguage)
        {
            if (!CacheData.Languages.Contains(newLanguage)) return false;
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
            return CacheData.Languages;
        }

        /// <summary>
        /// Return all Categories
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllCategories()
        {
            //CacheData.LoadCacheData();
            return CacheData.Categories;
        }

        /// <summary>
        /// Return all Keys
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
            //CacheData.LoadCacheData();
            if (!CacheData.IsDataLoaded) return null;
            var keys = CacheData.Keys;

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
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using TMPro;
using UnityEngine;
//TODO: Quitar dependencia de la API con EditorStrings
using static LocalizationTool.Scripts.Commons.EditorStrings;

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

            LocalizationManager.Instance.Init(()=>
            {
                LocalizationManager.Log(API_INITIALIZED_LOG);
                UpdateAllAddons(ActiveLanguage);
            });
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
            if (!LocalizationManager.IsDataLoaded) return "";
            if (key.IsNull()) return "";
            
            found = LocalizationManager.DictionaryCache.ContainsKey(key);
            if(found) found = LocalizationManager.DictionaryCache[key].TranslationData.ContainsKey(ActiveLanguage);
            return found
                ? LocalizationManager.DictionaryCache[key].TranslationData[ActiveLanguage]
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
            if (!LocalizationManager.Languages.Contains(language))
            {
                found = false;
                return "";
            }

            found = LocalizationManager.DictionaryCache[key].TranslationData.ContainsKey(language);
            return found
                ? LocalizationManager.DictionaryCache[key].TranslationData[ActiveLanguage]
                : "";
        }

        /// <summary>
        /// Change active language to an available language. All addon will be updated to the new language
        /// </summary>
        /// <param name="newLanguage">An available language</param>
        /// <returns>Return true if newLanguage exist, otherwise return false</returns>
        public static bool ChangeLanguage(string newLanguage)
        {
            if (!LocalizationManager.Languages.Contains(newLanguage)) return false;
            ActiveLanguage = newLanguage;
            UpdateAllAddons(newLanguage);
            return true;
        }

        public static List<string> GetAvailableLanguages()
        {
            return LocalizationManager.Languages;
        }

        /// <summary>
        /// Return all Categories
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllCategories()
        {
#pragma warning disable CS4014
            LocalizationManager.Instance.Init();
#pragma warning restore CS4014
            return LocalizationManager.Categories;
        }

        /// <summary>
        /// Return all Keys
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
#pragma warning disable CS4014
            LocalizationManager.Instance.Init();
#pragma warning restore CS4014
            if (!LocalizationManager.IsDataLoaded) return null;
            var keys = LocalizationManager.Keys;

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
            var tmps = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
            _addons = new List<LocalizationToolAddon>();
            foreach (var obj in tmps)
            {
                var item = (TextMeshProUGUI)obj;
                var addon = item.GetComponent<LocalizationToolAddon>();
                if (addon.IsNotNull()) _addons.Add(addon);
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Manager;
using UnityEngine;

namespace LocalizationTool.Controller
{
    //TODO: make this persistant between scenes
    //TODO: add this to the scene
    [DefaultExecutionOrder(-999)]
    public class LocalizationToolController : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public static LocalizationToolController Instance => _instance ??= (LocalizationToolController) FindObjectOfType(typeof(LocalizationToolController));

        public string ActiveLanguage { get; set; }

        public Action<string> OnLanguageUpdate { get; set; }
        
        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationToolController _instance;

        #endregion

        #region UNITY METHODS

        private void Awake()
        {
            LocalizationManager.Instance.Init();
        }

        private void Start()
        {
            OnLanguageUpdate.Invoke(ActiveLanguage);
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Return the value of a key in the current language
        /// </summary>
        /// <param name="key">Key to get value from</param>
        /// <returns>Value </returns>
        /// <exception cref="Exception">Thrown when key not found</exception>
        public string GetValueByKey(string key)
        {
            return LocalizationManager.Dictionary[key].LanguagesData.ContainsKey(ActiveLanguage)
                ? LocalizationManager.Dictionary[key].LanguagesData[ActiveLanguage]
                : throw new Exception($"Key not found in language '{ActiveLanguage}'");
        }

        /// <summary>
        /// Return the value of a key in the given language
        /// </summary>
        /// <param name="key">Key to get value from</param>
        /// <param name="language">Language to get value from</param>
        /// <returns>The value of the key</returns>
        public string GetValueByKeyAndLanguage(string key, string language)
        {
            if (!LocalizationManager.ActiveLanguages.Contains(language)) throw new Exception($"Language '{language}' doesnt exist");
                
            return LocalizationManager.Dictionary[key].LanguagesData.ContainsKey(language)
                ? LocalizationManager.Dictionary[key].LanguagesData[ActiveLanguage]
                : throw new Exception($"Key not found in language '{language}'");
        }

        /// <summary>
        /// Change active language to an available language
        /// </summary>
        /// <param name="newLanguage">An available language</param>
        /// <returns>Return true if newLanguage exist, otherwise return false</returns>
        public bool ChangeLanguage(string newLanguage)
        {
            if (!LocalizationManager.ActiveLanguages.Contains(newLanguage)) return false;
            
            ActiveLanguage = newLanguage;
            OnLanguageUpdate.Invoke(newLanguage);
            return true;
        }

        public List<string> GetAvailableLanguages()
        {
            return LocalizationManager.ActiveLanguages;
        }

        public List<string> GetAllCategories()
        {
            LocalizationManager.Instance.Init();
            var categories = LocalizationManager.Categories;
            return categories;
        }

        public List<string> GetAllKeys()
        {
            LocalizationManager.Instance.Init();
            var keys = LocalizationManager.Dictionary.Keys.ToList();
            return keys;
        }
        
        #endregion

        #region PRIVATE METHODS

        #endregion


    }
}
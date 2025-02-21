using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{
    [Serializable]
    public class LanguageDataSO : ScriptableObject
    {
        public string languageName;
        public int displayOrder;
        public bool isDefault;

        public List<TranslationKeyDataSO> translationKeys = new List<TranslationKeyDataSO>();
        public List<TranslationKeyDataSO> translationsOrdered => translationKeys.OrderBy(x => x.displayOrder).ToList();

        private Dictionary<string, TranslationKeyDataSO> cacheDictionary;

        public Dictionary<string, TranslationKeyDataSO> translationDictionary
        {
            get
            {
                if (cacheDictionary == null)
                {
                    BuildDictionary();
                }

                return cacheDictionary;
            }
        }

        private void BuildDictionary()
        {
            cacheDictionary = translationKeys.ToDictionary(x => x.keyName, x => x);
        }
    }
}
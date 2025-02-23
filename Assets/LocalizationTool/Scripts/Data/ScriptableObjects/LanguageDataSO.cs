using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.Commons;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{
    [Serializable]
    public class LanguageDataSO : ScriptableObject
    {
        public string languageName;
        public int displayOrder;
        public bool isDefault;

        public List<TranslationKeyDataSO> translationKeys = new();

        private Dictionary<string, TranslationKeyDataSO> _cacheDictionary = new();

        public Dictionary<string, TranslationKeyDataSO> TranslationDictionary
        {
            get
            {
                if (_cacheDictionary.IsNotNull() || translationKeys.Count != _cacheDictionary.Count)
                {
                    BuildDictionary();
                }

                return _cacheDictionary;
            }
        }

        private void BuildDictionary()
        {
            _cacheDictionary = translationKeys.ToDictionary(x => x.keyData.keyName, x => x);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace LocalizationTool.Scripts.Data.TemplatesForSerializer
{
    [Serializable]
    public class DictionaryTemplate
    {
        public List<KeyCategoryLanguageValues> dictionaryKeyCategoryLanguages = new();

        [Serializable]
        public class KeyCategoryLanguageValues
        {
            public string key;
            public string category;
            public List<LanguageValue> languageValues;
        }
        
        [Serializable]
        public class LanguageValue
        {
            public string Language;
            public string Value;
    
            public void Deconstruct(out string language, out string value)
            {
                language = Language;
                value = Value;
            }
        }
    }
}
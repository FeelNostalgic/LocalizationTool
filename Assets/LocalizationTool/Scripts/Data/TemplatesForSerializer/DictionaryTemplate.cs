using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace LocalizationTool.Scripts.Data.TemplatesForSerializer
{
    /// <summary>
    /// Used to serialize data to JSON and XML
    /// </summary>
    [Serializable]
    public class DictionaryTemplate
    {
        public List<string> languages = new();
        public List<string> categories = new();
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
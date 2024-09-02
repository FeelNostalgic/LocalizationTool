using System;
using System.Collections.Generic;

namespace LocalizationTool.Data.Templates
{
    [Serializable]
    public class DictionaryTemplate
    {
        public List<KeyCategoryLanguageValues> DictionaryKeyCategoryLanguages = new();

        [Serializable]
        public class KeyCategoryLanguageValues
        {
            public string Key;
            public string Category;
            public List<LanguageValue> LanguageValues;
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
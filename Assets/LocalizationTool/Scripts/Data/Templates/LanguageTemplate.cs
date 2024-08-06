using System;
using System.Collections.Generic;

namespace LocalizationTool.Data.Templates
{
    [Serializable]
    public class LanguageTemplate
    {
        public string Language;
        public List<KeyCategoryLanguage> Data = new();

        [Serializable]
        public class KeyCategoryLanguage
        {
            public string Key;
            public string Category;
            public string Value;
        }
            
    }
    
    
}
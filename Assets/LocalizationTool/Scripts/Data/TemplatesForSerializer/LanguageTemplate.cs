using System;
using System.Collections.Generic;

namespace LocalizationTool.Scripts.Data.TemplatesForSerializer
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
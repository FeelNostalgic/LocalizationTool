using System;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class ConfigurationData
    {
        public bool DictionaryDeleteConfirmation;
        public bool LanguageDeleteConfirmation;
        public bool CategoryDeleteConfirmation;

        public bool DictionaryClearAdd;
        public bool LanguageClearAdd;
        public bool CategoryClearAdd;
        
        public int SearchTypeIndex;

        public bool ShowLogsInConsole;
    }
}
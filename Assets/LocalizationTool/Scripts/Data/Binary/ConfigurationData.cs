using System;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class ConfigurationData
    {
        public bool dictionaryDeleteConfirmation;
        public bool languageDeleteConfirmation;
        public bool categoryDeleteConfirmation;

        public bool dictionaryClearAdd;
        public bool languageClearAdd;
        public bool categoryClearAdd;
        
        public int searchTypeIndex;

        public bool showLogsInConsole;
    }
}
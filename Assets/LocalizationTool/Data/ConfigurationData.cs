using System;

namespace LocalizationTool.Data
{
    [Serializable]
    public class ConfigurationData
    {
        public bool DictionaryDeleteConfirmation;
        public bool LanguageDeleteConfirmation;
        public bool CategoryDeleteConfirmation;

        public int SearchTypeIndex;

        public bool ShowLogsInConsole;
    }
}
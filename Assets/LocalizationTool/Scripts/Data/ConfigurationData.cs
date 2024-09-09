#if UNITY_EDITOR
using System;

namespace LocalizationTool.Scripts.Data
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
#endif
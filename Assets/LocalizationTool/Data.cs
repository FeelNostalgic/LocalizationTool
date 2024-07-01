using System.Collections.Generic;

namespace LocalizationTool
{
    public class Data
    {
        public enum GROUPS
        {
            None = 0,
            MainMenu = 1,
            CharacterMenu = 2,
            Dialogs = 3
        }

        public enum LANGUAGES
        {
            Spanish = 0,
            English = 1,
            Italian = 2
        }

        public enum GUI_SECTIONS
        {
            valueFeedback = 0,
            removeFeedback = 1,
            languageFeedback = 2
        }
    }
    
    public struct KeyData
    {
        public Data.GROUPS Group;
        public Dictionary<Data.LANGUAGES, string> LanguagesData;
    }
}
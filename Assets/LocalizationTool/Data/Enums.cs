using System.Collections.Generic;

namespace LocalizationTool.Data
{
    public class Enums
    {
        public enum GROUPS
        {
            None = 0,
            MainMenu = 1,
            CharacterMenu = 2,
            Dialogs = 3
        }
        
        public enum GUI_WINDOW
        {
            Dictionary = 0,
            Languages = 1,
            Categories = 2,
            Configuration = 3
        }
    }
    
    public struct KeyData
    {
        public Enums.GROUPS Category;
        public Dictionary<string, string> LanguagesData;
    }
}
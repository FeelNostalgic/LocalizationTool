using System.Collections.Generic;

namespace LocalizationTool.Data
{
    public class Enums
    {
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
        public string Category;
        public Dictionary<string, string> LanguagesData;
    }
}
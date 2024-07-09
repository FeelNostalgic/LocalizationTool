using System;
using System.Collections.Generic;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class LanguagesData
    {
        public string FavouriteLanguage;
        public List<string> Languagues = new();

        public bool Contains(string language)
        {
            return Languagues.Contains(language);
        }
    }
}
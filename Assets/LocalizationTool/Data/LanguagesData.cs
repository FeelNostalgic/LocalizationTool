using System;
using System.Collections.Generic;

namespace LocalizationTool.Data
{
    [Serializable]
    public class LanguagesData
    {
        public string FavouriteLanguage;
        public List<string> Languagues = new();
    }
}
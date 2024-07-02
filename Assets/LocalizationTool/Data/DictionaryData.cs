using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LocalizationTool.Data
{
    [Serializable]
    public class DictionaryData
    {
        public DictionaryData()
        {
            ListDictionaryKeyCategoryLanguages = new List<KeyCategoryLanguage>();
        }
        
        public List<KeyCategoryLanguage> ListDictionaryKeyCategoryLanguages;
    }

    [Serializable]
    public class KeyCategoryLanguage
    {
        public KeyCategoryLanguage(string key, string category, List<LanguageValue> languageValue)
        {
            Key = key;
            Category = category;
            _languagesValue = languageValue;
        }
        
        public string Key;
        public string Category;
        public Dictionary<string, string> DictionaryLanguageValue => _languagesValue.ToDictionary(x => x.Language, x => x.Value);

        [SerializeField] private List<LanguageValue> _languagesValue;

        public void AddNewLanguage(string newLanguage)
        {
            _languagesValue.Add(new LanguageValue{Language = newLanguage, Value = ""});
        }
        
        public void UpdateValue(string language, string newValue)
        {
            _languagesValue ??= new List<LanguageValue>();
            var data = _languagesValue.FirstOrDefault(x => x.Language == language);
            if(data != default) _languagesValue.Remove(data);
            _languagesValue.Add(new LanguageValue{Language = language, Value = newValue});
        }

        public Task UpdateLanguageName(string oldLanguageName, string newLanguageName)
        {
            if (_languagesValue.FirstOrDefault(l => l.Language.Equals(oldLanguageName)) != default)
            {
                _languagesValue.First(l => l.Language.Equals(oldLanguageName)).Language = newLanguageName;
            }
           
            return Task.CompletedTask;
        }

        public Task RemoveLanguage(string languageToRemove)
        {
            _languagesValue.Remove(_languagesValue.Find(l => l.Language.Equals(languageToRemove)));
            return Task.CompletedTask;
        }

        public Task UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            if(Category.Equals(oldCategoryName)) Category = newCategoryName;
            return Task.CompletedTask;
        }

        public Task RemoveCategory(string categoryToRemove)
        {
            if(Category.Equals(categoryToRemove)) Category = "None";
            return Task.CompletedTask;
        }
        
    }
    
    [Serializable]
    public class LanguageValue
    {
        public string Language;
        public string Value;
    }
}
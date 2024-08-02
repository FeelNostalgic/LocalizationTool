using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Manager;
using UnityEngine;

namespace LocalizationTool.Data.Json
{
    [Serializable]
    public class DictionaryData
    {
        public DictionaryData()
        {
            ListDictionaryKeyCategoryLanguages = new List<KeyCategoryLanguageValues>();
        }
        
        public List<KeyCategoryLanguageValues> ListDictionaryKeyCategoryLanguages;

        public void AddNewKeyCategoryLanguage(KeyCategoryLanguageValues item)
        {
            if (ListDictionaryKeyCategoryLanguages.FirstOrDefault(i => i.Key == item.Key) != default)
            {
                LocalizationManager.Log($"When importing: Key {item.Key} is already in the dictionary");
                return;
            }
            ListDictionaryKeyCategoryLanguages.Add(item);
        }
        
        public void UpdateCategoryName(string key, string newCategory)
        {
            var item = ListDictionaryKeyCategoryLanguages.First(k => k.Key == key);
            item.Category = newCategory;
        }

        public void UpdateLanguageValue(string key, string language, string value)
        {
            var item = ListDictionaryKeyCategoryLanguages.First(k => k.Key == key);
            item.UpdateValue(language, value);
        }
    }

    [Serializable]
    public class KeyCategoryLanguageValues
    {
        public KeyCategoryLanguageValues(string key, string category, List<LanguageValue> languageValue)
        {
            Key = key;
            Category = category;
            _languagesValue = languageValue;
        }
        
        public string Key;
        public string Category;
        [SerializeField] private List<LanguageValue> _languagesValue;
        public Dictionary<string, string> DictionaryLanguageValue => _languagesValue.ToDictionary(x => x.Language, x => x.Value);
        
        public void AddNewLanguage(string newLanguage)
        {
            _languagesValue.Add(new LanguageValue{Language = newLanguage, Value = ""});
        }

        public void UpdateKey(string newValue)
        {
            Key = newValue;
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

        public void UpdateCategory(string newCategoryName)
        {
            Category = newCategoryName;
        }
        
        public Task UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            if(Category.Equals(oldCategoryName)) Category = newCategoryName;
            return Task.CompletedTask;
        }

        public Task RemoveCategory(string categoryToRemove)
        {
            if(Category.Equals(categoryToRemove)) Category = LocalizationManager.DefaultCategory;
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
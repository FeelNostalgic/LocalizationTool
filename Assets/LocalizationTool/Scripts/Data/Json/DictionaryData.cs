using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Scripts.General;
using UnityEngine;

namespace LocalizationTool.Data.Json
{
    [Serializable]
    public class DictionaryData
    {
        public List<KeyCategoryLanguageValues> listDictionaryKeyCategoryLanguages;
        
        public DictionaryData()
        {
            listDictionaryKeyCategoryLanguages = new List<KeyCategoryLanguageValues>();
        }

        public void AddNewKeyCategoryLanguage(KeyCategoryLanguageValues item)
        {
            if (listDictionaryKeyCategoryLanguages.FirstOrDefault(i => i.key.Equals(item.key)) != default)
            {
                LocalizationManager.Log($"When importing: Key {item.key} is already in the dictionary");
                return;
            }
            listDictionaryKeyCategoryLanguages.Add(item);
        }
        
        public void UpdateCategoryName(string key, string newCategory)
        {
            var item = listDictionaryKeyCategoryLanguages.First(k => k.key.Equals(key));
            item.category = newCategory;
        }

        public void UpdateLanguageValue(string key, string language, string value)
        {
            var item = listDictionaryKeyCategoryLanguages.First(k => k.key.Equals(key));
            item.UpdateValue(language, value);
        }

        public void RemoveKey(string key)
        {
            var dataToRemove = listDictionaryKeyCategoryLanguages.First(x => x.key.Equals(key));
            listDictionaryKeyCategoryLanguages.Remove(dataToRemove);
        }

        public void Clear()
        {
            listDictionaryKeyCategoryLanguages.Clear();
        }
    }

    [Serializable]
    public class KeyCategoryLanguageValues
    {
        public KeyCategoryLanguageValues(string key, string category, List<LanguageValue> languageValue)
        {
            this.key = key;
            this.category = category;
            languagesValue = languageValue;
        }
        
        public string key;
        public string category;
        [SerializeField] private List<LanguageValue> languagesValue;
        public Dictionary<string, string> DictionaryLanguageValue => languagesValue.ToDictionary(x => x.language, x => x.value);
        
        public void AddNewLanguage(string newLanguage)
        {
            languagesValue.Add(new LanguageValue{language = newLanguage, value = ""});
        }

        public void UpdateKey(string newValue)
        {
            key = newValue;
        }
        
        public void UpdateValue(string language, string newValue)
        {
            languagesValue ??= new List<LanguageValue>();
            var data = languagesValue.FirstOrDefault(x => x.language == language);
            if(data != default) languagesValue.Remove(data);
            languagesValue.Add(new LanguageValue{language = language, value = newValue});
        }

        public Task UpdateLanguageName(string oldLanguageName, string newLanguageName)
        {
            if (languagesValue.FirstOrDefault(l => l.language.Equals(oldLanguageName)) != default)
            {
                languagesValue.First(l => l.language.Equals(oldLanguageName)).language = newLanguageName;
            }
           
            return Task.CompletedTask;
        }

        public Task RemoveLanguage(string languageToRemove)
        {
            languagesValue.Remove(languagesValue.Find(l => l.language.Equals(languageToRemove)));
            return Task.CompletedTask;
        }

        public void UpdateCategory(string newCategoryName)
        {
            category = newCategoryName;
        }
        
        public Task UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            if(category.Equals(oldCategoryName)) category = newCategoryName;
            return Task.CompletedTask;
        }

        public Task RemoveCategory(string categoryToRemove)
        {
            if(category.Equals(categoryToRemove)) category = LocalizationManager.GetDefaultCategory();
            return Task.CompletedTask;
        }
        
    }
    
    [Serializable]
    public class LanguageValue
    {
        public string language;
        public string value;
    }
}
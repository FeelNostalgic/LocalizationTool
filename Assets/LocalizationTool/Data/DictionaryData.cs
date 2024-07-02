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
            ListDictionaryKeyValue = new List<KeyValue>();
        }
        
        public List<KeyValue> ListDictionaryKeyValue;
    }

    [Serializable]
    public class KeyValue
    {
        public KeyValue(string key, Enums.GROUPS category, List<LanguageValue> languageValue)
        {
            Key = key;
            Category = category;
            _languagesValue = languageValue;
        }

        public void UpdateValue(string language, string newValue)
        {
            _languagesValue ??= new List<LanguageValue>();
            var data = _languagesValue.FirstOrDefault(x => x.Language == language);
            if(data != default) _languagesValue.Remove(data);
            _languagesValue.Add(new LanguageValue{Language = language, Value = newValue});
        }

        public Task RemoveLanguage(string languageToRemove)
        {
            _languagesValue.First(l => l.Language.Equals(languageToRemove)).Value = "";
            return Task.CompletedTask;
        }

        public string Key;
        public Enums.GROUPS Category;
        public Dictionary<string, string> DictionaryLanguageValue => _languagesValue.ToDictionary(x => x.Language, x => x.Value);

        [SerializeField] private List<LanguageValue> _languagesValue;
    }
    
    [Serializable]
    public class LanguageValue
    {
        public string Language;
        public string Value;
    }
}
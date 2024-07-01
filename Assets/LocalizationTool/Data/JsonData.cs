using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LocalizationTool
{
    [Serializable]
    public class JsonData
    {
        public JsonData()
        {
            ListDictionaryKeyValue = new List<KeyValue>();
        }
        
        public List<KeyValue> ListDictionaryKeyValue;
    }

    [Serializable]
    public class KeyValue
    {
        public KeyValue(string key, Data.GROUPS group, List<LanguageValue> languageValue)
        {
            Key = key;
            Group = group;
            _languagesValue = languageValue;
        }

        public void UpdateValue(Data.LANGUAGES language, string newValue)
        {
            _languagesValue ??= new List<LanguageValue>();
            var data = _languagesValue.FirstOrDefault(x => x.Language == language);
            if(data != default) _languagesValue.Remove(data);
            _languagesValue.Add(new LanguageValue{Language = language, Value = newValue});
        }

        public string Key;
        public Data.GROUPS Group;
        public Dictionary<Data.LANGUAGES, string> DictionaryLanguageValue => _languagesValue.ToDictionary(x => x.Language, x => x.Value);

        [SerializeField] private List<LanguageValue> _languagesValue;
    }
    
    [Serializable]
    public class LanguageValue
    {
        public Data.LANGUAGES Language;
        public string Value;
    }
}
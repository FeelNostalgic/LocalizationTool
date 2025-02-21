using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{
    [Serializable]
    public class LanguageDataSO : ScriptableObject
    {
        public string languageName;
        public int displayOrder;
        public bool isDefault;
        
        public List<TranslationKeyDataSO> translationKeys = new List<TranslationKeyDataSO>();
    }
}
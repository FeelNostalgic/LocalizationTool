using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class LocalizationDataSO : ScriptableObject
	{
		public List<LanguageDataSO> languages = new List<LanguageDataSO>();
		public List<CategoryDataSO> categories = new List<CategoryDataSO>();
		public List<TranslationKeyDataSO> translationKeys = new List<TranslationKeyDataSO>();
	}
}
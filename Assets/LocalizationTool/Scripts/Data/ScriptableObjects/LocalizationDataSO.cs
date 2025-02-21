using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.Commons;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class LocalizationDataSO : ScriptableObject
	{
		public List<LanguageDataSO> languages = new();
		public List<CategoryDataSO> categories = new();
		public List<KeyDataSO> keys = new();
		
		private Dictionary<string, LanguageDataSO> _languageCacheDictionary = new();
		private Dictionary<string, CategoryDataSO> _categoryCacheDictionary = new();
		private Dictionary<string, KeyDataSO> _keyCacheDictionary = new();
		
		public Dictionary<string, LanguageDataSO> LanguagesDictionary
		{
			get
			{
				if (_languageCacheDictionary.IsNotNull() || languages.Count != _languageCacheDictionary.Count) BuildLanguagesDictionary();
				return _languageCacheDictionary;
			}
		}

		public Dictionary<string, CategoryDataSO> CategoriesDictionary
		{
			get
			{
				if (_categoryCacheDictionary.IsNotNull() || categories.Count != _categoryCacheDictionary.Count) BuildCategoriesDictionary();
				return _categoryCacheDictionary;
			}
		}
		
		public Dictionary<string, KeyDataSO> KeysDictionary
		{
			get
			{
				if (_keyCacheDictionary.IsNotNull() || keys.Count != _keyCacheDictionary.Count) BuildKeysDictionary();
				return _keyCacheDictionary;
			}
		}
		
		private void BuildLanguagesDictionary()
		{
			_languageCacheDictionary = languages.ToDictionary(x => x.languageName, x => x);
		}

		private void BuildCategoriesDictionary()
		{
			_categoryCacheDictionary = categories.ToDictionary(x => x.categoryName, x => x);
		}

		private void BuildKeysDictionary()
		{
			_keyCacheDictionary = keys.ToDictionary(x=> x.keyName, x=> x);
		}
	}
}
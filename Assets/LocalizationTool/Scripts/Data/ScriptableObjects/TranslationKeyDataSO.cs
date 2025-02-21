using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class TranslationKeyDataSO : ScriptableObject
	{
		public string keyName;
		public CategoryDataSO category;
		public int displayOrder;
		public Dictionary<string, string> translations = new Dictionary<string, string>();
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class TranslationKeyDataSO : ScriptableObject
	{
		public KeyDataSO keyData;
		public string translationText;
	}
}
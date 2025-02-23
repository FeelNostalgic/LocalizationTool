using System;
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
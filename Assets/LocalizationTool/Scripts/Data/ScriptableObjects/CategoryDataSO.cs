using System;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class CategoryDataSO : ScriptableObject
	{
		public string categoryName;
		public int displayOrder;
		public bool isDefault;
	}
}
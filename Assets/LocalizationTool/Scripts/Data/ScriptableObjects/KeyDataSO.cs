using System;
using UnityEngine;

namespace LocalizationTool.Scripts.Data.ScriptableObjects
{ 
	[Serializable]
	public class KeyDataSO : ScriptableObject
	{
		public string keyName;
		public CategoryDataSO category;
		public int displayOrder;
		
		public string CategoryName => category != null ? category.categoryName : string.Empty;
	}
}
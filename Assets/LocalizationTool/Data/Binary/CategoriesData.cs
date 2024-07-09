using System;
using System.Collections.Generic;

namespace LocalizationTool.Data.Binary
{ 
	[Serializable]
	public class CategoriesData 
	{
		public List<string> Categories = new();

		public bool Contains(string category)
		{
			return Categories.Contains(category);
		}
	}
}
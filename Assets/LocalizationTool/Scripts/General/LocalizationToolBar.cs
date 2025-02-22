#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;

namespace LocalizationTool.Scripts.General
{ 
	public abstract class LocalizationToolBar 
	{
		[MenuItem("Tools/LocalizationTool/Install", false, -100)]
		public static void Install(MenuCommand menuCommand)
		{
			if (FindObjectOfType<LocalizationToolAPI>().IsNull()) {
				EditorUtils.MenuItemNewObject<LocalizationToolAPI>(menuCommand, "LocalizationToolAPI");
				LocalizationToolAPI.ActiveLanguage = CacheDataSO.DefaultLanguage;
			} else {
				Debug.LogWarning("LocalizationToolAPI already exists");
			}
		}
		
		[MenuItem("Tools/LocalizationTool/UI/Add Addon To TMPro", false, 0)]
		public static void AddAddon(MenuCommand menuCommand)
		{
			EditorUtils.MenuItemSetInObject<LocalizationToolAddon>(menuCommand);
		}
	}
}

#endif
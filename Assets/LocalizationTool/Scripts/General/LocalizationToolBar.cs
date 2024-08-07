using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using static UnityEngine.Object;

namespace LocalizationTool.Scripts.General
{ 
	public abstract class LocalizationToolBar 
	{
		[MenuItem("Tools/LocalizationTool/Install", false, -100)]
		public static void Install(MenuCommand menuCommand)
		{
			if (FindObjectsOfType<LocalizationToolAPI>().IsNull()) {
#pragma warning restore CS0618
				EditorUtils.MenuItemNewObject<LocalizationToolAPI>(menuCommand, "LocalizationToolAPI");
				LocalizationToolAPI.ActiveLanguage = LocalizationManager.DefaultLanguage;
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
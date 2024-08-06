using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Scripts.Editors
{ 
	public class InfoEditor : LocalizationEditor
	{
		#region DIMENSION VARIABLES

		private static readonly Vector2 WindowSize = new(1350, 800);

		#endregion

		#region PRIVATE VARIABLES

		private string _title;
		private string _text;

		private Vector2 _scroll;
		
		#endregion

		public static void ShowWindow(string title, string text)
		{
			var window = GetWindow<InfoEditor>(title);
			window.minSize = WindowSize;
			window._title = title;
			window._text = text;
		}
		
		#region PRIVATE METHODS

		private new void OnGUI()
		{
			GUILayout.BeginVertical();

			GUILayout.Space(5);
			ShowHeader1(_title);
			GUILayout.Space(5);
			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
			var style = new GUIStyle(EditorStyles.textArea)
			{
				alignment = TextAnchor.UpperLeft,
				fontSize = 13
			};
			EditorGUILayout.SelectableLabel(_text, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			EditorGUILayout.EndScrollView();
			
			GUILayout.EndVertical();
		}
		
		#endregion
	}
}
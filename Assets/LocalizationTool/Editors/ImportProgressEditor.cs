
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{ 
	public class ImportProgressEditor : EditorWindow
	{
		private float progress = 0f;
		private string status = "Importing Data...";

		public void SetProgress(float progress)
		{
			this.progress = progress;
			Repaint(); 
		}

		private void OnGUI()
		{
			GUILayout.Label(status);
			GUILayout.HorizontalSlider(progress, 0f, 1f);
		}
		
		public static ImportProgressEditor OpenWindow(string title)
		{
			ImportProgressEditor window = EditorWindow.GetWindow<ImportProgressEditor>();
			window.titleContent = new GUIContent(title);
			return window;
		}
	}
}
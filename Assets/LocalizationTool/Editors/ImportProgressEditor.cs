
using LocalizationTool.Commons;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{ 
	public class ImportProgressEditor : EditorWindow
	{
		#region PRIVATE VARIABLES

		private float _progress = 0f;
		private string _status = "Starting import data...";
		private string _progressInfo;
		private string _result;
		private bool _isCompleted;

		private readonly Vector2 _windowSize = new(400,140);

		#endregion
		
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (!_isCompleted)
			{
				GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			
				GUILayout.Label(_status, CustomStyles.GetStyle(Enums.CustomStyleName.FeedbackLabel), GUILayout.Height(35), GUILayout.Width(position.width-6));
				GUILayout.Label(_progressInfo, CustomStyles.GetStyle(Enums.CustomStyleName.FeedbackLabel), GUILayout.Height(35), GUILayout.Width(position.width-6));
				EditorGUI.ProgressBar(new Rect(3, 35+35+10, position.width - 6, 30), _progress, "Progress");
			
				GUILayout.EndVertical();
			}
			else
			{
				GUILayout.BeginVertical("box",GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			
				GUILayout.FlexibleSpace();
				GUILayout.Label(_result, CustomStyles.GetStyle(Enums.CustomStyleName.FeedbackLabel), GUILayout.Height(70), GUILayout.Width(position.width-6));
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Close", CustomStyles.GetStyle(Enums.CustomStyleName.CloseRichTextEditorButton))) Close();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		
		#region PUBLIC METHODS

		public static ImportProgressEditor OpenWindow(string title)
		{
			var window = GetWindow<ImportProgressEditor>();
			window.titleContent = new GUIContent(title);
			window.maxSize = window._windowSize;
			window.minSize = window._windowSize;
			window._isCompleted = false;
			return window;
		}

		public void SetStatus(string status)
		{
			_status = status;
			Repaint(); 
		}

		public void SetProgressInfo(string info)
		{
			_progressInfo = info;
			Repaint();
		}
		
		public void SetProgress(float progress)
		{
			_progress = progress;
			Repaint(); 
		}

		public void Complete(string result)
		{
			_isCompleted = true;
			_result = result;
			Repaint();
		}

		#endregion


		#region PRIVATE METHODS

		

		#endregion
	}
}
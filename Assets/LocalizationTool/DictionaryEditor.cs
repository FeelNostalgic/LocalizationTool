using UnityEngine;

namespace LocalizationTool.Editor
{ 
	public class DictionaryEditor : LocalizationEditor
	{
		#region Inspector Variables
		
		#endregion
	
		#region Public Variables
		
		#endregion

		#region Private Variables
		
		#region DIMENSION VARIABLES

		
		private float _leftSectionWidthPercent = 0.225f;
		private float _rigthSectionWidthPercent = 0.225f;

		private float _dividerPosition = 305f;
		private const float dividerWidth = 5f;

		private bool isResizingDivider = false;

		#endregion
		
		#endregion
		
		#region Public Methods
		
		public void ShowDictionaryLayout()
		{
			GUILayout.BeginVertical(MinHeightOption(_windowSize.y));
            
			//GUILayout.Space(10);
			ShowHorizontalLine(5);
            
			GUILayout.BeginHorizontal();
			ShowLeftSection();

			ShowVerticalLine(5);
			// TODO: VerticalReDimensionalDivisionLine(_leftSectionWidth);

			//ShowCenterSection();
            
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		
		#endregion

		#region Private Methods
		
		private void ShowLeftSection()
		{
			GUILayout.BeginVertical(MinWidthOption(GetWidthSize(_leftSectionWidthPercent)));
			//ShowAddSection();
			GUILayout.EndVertical();
		}
		
		#endregion
	}
}
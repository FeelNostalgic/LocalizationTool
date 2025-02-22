#if UNITY_EDITOR

using System;
using System.Collections;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.GameUtils;

namespace LocalizationTool.Scripts.Editors
{ 
	public abstract class EditorWindowAbstract 
	{
        #region DIMENSION VARIABLES

        protected static readonly Vector2 WindowSize = new(1400, 1000);
        protected readonly GUILayoutOption Height = GUILayout.Height(40);
        
        #endregion
        
		#region TEXTURE VARAIBLES

		protected static Texture RefreshIcon => EditorGUIUtility.IconContent("d_Refresh").image;
		protected static Texture AddIcon => EditorGUIUtility.IconContent("d_ol_plus").image;
		protected static Texture EditIcon => EditorGUIUtility.IconContent("Customized").image;
		protected static Texture DeleteIcon => EditorGUIUtility.IconContent("d_TreeEditor.Trash").image;
		protected static Texture UpIcon => EditorGUIUtility.IconContent("d_scrollup").image;
		protected static Texture DownIcon => EditorGUIUtility.IconContent("d_scrolldown").image;
		protected static Texture StarIcon => EditorGUIUtility.IconContent("d_Favorite").image;
		public static Texture ImportIcon => EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
		public static Texture ExportIcon => EditorGUIUtility.IconContent("d_SaveAs").image;
		protected static Texture LockIcon => EditorGUIUtility.IconContent("d_AssemblyLock").image;
		protected static Texture GearIcon => EditorGUIUtility.IconContent("d__Popup").image;
		public static Texture WarningIcon => EditorGUIUtility.IconContent("d_console.warnicon.sml").image;
		protected static Texture InfoIcon => EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image;
		public static Texture ChangesSavedIcon => EditorGUIUtility.IconContent("d_CacheServerConnected").image;
		public static Texture AddEmptyIcon => EditorGUIUtility.IconContent("d_ol_plus_act").image;
        public static Texture MinusEmptyIcon => EditorGUIUtility.IconContent("d_ol_minus_act").image;
        public static Texture ApplyStyleIcon => EditorGUIUtility.IconContent("d_Progress").image;
        public static Texture UndoIcon => EditorGUIUtility.IconContent("d_scrollleft").image;
        public static Texture RedoIcon => EditorGUIUtility.IconContent("d_scrollright").image;
        public static Texture EmptyIcon => EditorGUIUtility.IconContent("d_UndoHistory").image;
        protected static Texture YellowStarIcon => GetColoredIcon("d_Favorite", Color.yellow);

		#endregion

        #region PROTECTED VARIABLES

        public static string FeedbackLabel = "";
        protected bool AddActionRunning;

        public static readonly string[] CsvSeparators = { ";", ",", ".", ":", "|", "=" };
        
        #endregion

        #region PRIVATE VARIABLES
        
        private EditorCoroutine _currentCoroutine;

        #endregion

        #region METHODS

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }

        public virtual void ShowLayout()
        {
        }

        protected virtual void ControlFocus(string focus)
        {
            if (GUI.GetNameOfFocusedControl() != focus) return;
            if (Event.current is { isKey: true }) EditorGUI.FocusTextInControl(focus);
        }

        public void ControlFeedbackLabel(string text)
        {
            AddActionRunning = true;
            if (_currentCoroutine != null) EditorCoroutineUtility.StopCoroutine(_currentCoroutine);
            _currentCoroutine = EditorCoroutineUtility.StartCoroutine(FeedbackLabelCoroutine(text, delegate(string s) { FeedbackLabel = s; }, 1.6f), this);
        }

        protected void ControlFeedbackLabelInRow(Action<string> labelUpdate, string text, Action onComplete)
        {
            if (_currentCoroutine != null) EditorCoroutineUtility.StopCoroutine(_currentCoroutine);
            _currentCoroutine = EditorCoroutineUtility.StartCoroutine(FeedbackLabelCoroutine(text, labelUpdate.Invoke, 2.5f, onComplete), this);
        }

        private IEnumerator FeedbackLabelCoroutine(string text, Action<string> updateFeedbackLabel, float duration, Action onComplete = null)
        {
            updateFeedbackLabel?.Invoke(text);
            yield return new WaitForSecondsRealtime(duration);
            updateFeedbackLabel?.Invoke("");
            LocalizationMainEditor.Instance.Repaint();
            _currentCoroutine = null;
            AddActionRunning = false;

            onComplete?.Invoke();
        }

        protected static void RepaintGUI()
        {
            LocalizationMainEditor.Instance.Repaint();
        }
        
        public virtual void ClearAddTextField()
        {
            
        }

        #endregion
        
		#region GUI ELEMENTS

        public static GUIContent GetGUIContent(Texture t, string tooltip)
        {
            return new GUIContent(t, tooltip);
        }

        public static void ShowHeader1(string text, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(text, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label), options);
            GUILayout.Space(10);
        }

        public static void ShowHeader2(string text, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(text, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label), options);
            GUILayout.Space(10);
        }

        protected static void ShowSubHeader(string text, string tooltip = "", params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            var content = new GUIContent(text, tooltip);
            GUILayout.Label(content, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleLeft15Label), options);
            GUILayout.Space(6);
        }

        protected static void ShowSectionHeader(string text, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(text, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter15Label), options);
            GUILayout.Space(10);
        }

        protected static void ShowLabelPopupSelectionCategory(string label, ref string categoryValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);

            if (CacheDataSO.CategoryCache != null) //To avoid possible errors while loading data
            {
                if (CacheDataSO.Categories.IndexOf(categoryValue) != -1) //Category value is empty when loading
                {
                    var index = EditorGUILayout.Popup(CacheDataSO.Categories.IndexOf(categoryValue), CacheDataSO.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
                    categoryValue = CacheDataSO.Categories[index];
                }
                else
                {
                    EditorGUILayout.Popup(0, CacheDataSO.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
                    if (CacheDataSO.CategoryCache.Count > 0) categoryValue = CacheDataSO.Categories[0];
                }
            }

            GUILayout.EndVertical();
        }

        protected static void ShowLabelTextFieldVertical(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUI.SetNextControlName(label);
            GUILayout.Label(label, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);
            keyValue = EditorGUILayout.TextField(keyValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));
            GUILayout.EndVertical();
        }

        protected static void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, CustomStyles.GetStyle(Enums.CustomStyleName.FeedbackLabel), GUILayout.Height(30));
        }

        protected static void ShowVerticalLine(float width)
        {
            GUI.backgroundColor = Colors.DEEP_GRAY;
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(width));
            GUI.backgroundColor = Colors.DEFAULT;
        }

        public static void ShowHorizontalLine(float height)
        {
            GUI.backgroundColor = Colors.DEEP_GRAY;
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
            GUI.backgroundColor = Colors.DEFAULT;
        }

        protected void VerticalReDimensionalDivisionLine(float width)
        {
            //FUTURE: VerticalReDimensionalDivisionLine
            // var dividerRect = new Rect(width, 0f, dividerWidth, position.height);
            // EditorGUIUtility.AddCursorRect(dividerRect, MouseCursor.ResizeHorizontal);
            // EditorGUI.DrawRect(dividerRect, Color.black);
            // RedimensionEvent(dividerRect);
        }

        protected void RedimensionEvent(Rect dividerRect)
        {
            //FUTURE: RedimensionEvent
            // isResizingDivider = Event.current.type switch
            // {
            //     EventType.MouseDown when dividerRect.Contains(Event.current.mousePosition) => true,
            //     EventType.MouseUp => false,
            //     _ => isResizingDivider
            // };
            //
            // if (!isResizingDivider) return;
            //
            // _dividerPosition = Event.current.mousePosition.x;
            // Repaint();
        }

        protected static float GetWidthSize(float percent, float WindowSizeX)
        {
            return percent * WindowSizeX;
        }

        protected static GUILayoutOption MinWidthOption(float width)
        {
            return GUILayout.MinWidth(width);
        }

        protected static GUILayoutOption MaxWidthOption(float width)
        {
            return GUILayout.MaxWidth(width);
        }

        protected static GUILayoutOption MinHeightOption(float height)
        {
            return GUILayout.MinHeight(height);
        }

        protected static GUILayoutOption MaxHeightOption(float height)
        {
            return GUILayout.MaxHeight(height);
        }

		#endregion
	}
}
#endif
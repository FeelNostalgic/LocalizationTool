using System;
using System.Collections;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.GameUtils;

namespace LocalizationTool.Scripts.Editors
{
#if UNITY_EDITOR
    public class LocalizationEditor : EditorWindow
    {
        #region PROTECTED VARIABLES

        protected static Texture RefreshIcon => EditorGUIUtility.IconContent("d_Refresh").image;
        protected static Texture AddIcon => EditorGUIUtility.IconContent("d_ol_plus").image;
        protected static Texture EditIcon => EditorGUIUtility.IconContent("Customized").image;
        protected static Texture DeleteIcon => EditorGUIUtility.IconContent("d_TreeEditor.Trash").image;
        protected static Texture UpIcon => EditorGUIUtility.IconContent("d_scrollup").image;
        protected static Texture DownIcon => EditorGUIUtility.IconContent("d_scrolldown").image;
        protected static Texture StarIcon => EditorGUIUtility.IconContent("d_Favorite").image;
        protected static Texture ImportIcon => EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
        protected static Texture ExportIcon => EditorGUIUtility.IconContent("d_SaveAs").image;
        protected static Texture LockIcon => EditorGUIUtility.IconContent("d_AssemblyLock").image;
        protected static Texture GearIcon => EditorGUIUtility.IconContent("d__Popup").image;
        protected static Texture WarningIcon => EditorGUIUtility.IconContent("d_console.warnicon.sml").image;
        protected static Texture InfoIcon => EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image;
        protected static Texture ChangesSavedIcon => EditorGUIUtility.IconContent("d_CacheServerConnected").image;
        protected static Texture AddEmptyIcon => EditorGUIUtility.IconContent("d_ol_plus_act").image;
        protected static Texture MinusEmptyIcon => EditorGUIUtility.IconContent("d_ol_minus_act").image;
        protected static Texture ApplyStyleIcon => EditorGUIUtility.IconContent("d_Progress").image;
        protected static Texture UndoIcon => EditorGUIUtility.IconContent("d_scrollleft").image;
        protected static Texture RedoIcon => EditorGUIUtility.IconContent("d_scrollright").image;
        protected static Texture YellowStarIcon => GetColoredIcon("d_Favorite", Color.yellow);

        protected string FeedbackLabel = "";
        protected bool AddActionRunning;

        protected readonly string[] CsvSeparators = { ";", ",", ".", ":", "|", "=" };

        #endregion

        #region EDITOR VARIABLES

        private int _currentWindowToolbarIndex;

        private Vector2 _scrollRight;

        private Enums.GUIWindow _currentWindow;
        private static DictionaryEditor _dictionaryEditor;
        private static LanguagesEditor _languagesEditor;
        private static CategoriesEditor _categoriesEditor;

        private static ConfigurationEditor _configurationEditor;

        private EditorCoroutine _currentCoroutine;

        #endregion

        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(1400, 1000);
        protected readonly GUILayoutOption Height = GUILayout.Height(40);

        #endregion

        [MenuItem("Tools/LocalizationTool/Manager", false, -30)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationEditor));
            window.minSize = WindowSize;
            window.titleContent = new GUIContent(MAIN_WINDOW_LABEL);
            LoadData();
        }

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            LoadData();
        }

        #region UNITY METHODS

        protected virtual void OnEnable()
        {
            //
        }

        // Called when unity editor is closed
        protected virtual void OnDisable()
        {
            //
        }

        // Called when editor is closed
        protected virtual void OnDestroy()
        {
            //
        }

        protected void RepaintGUI()
        {
            Repaint();
        }

        protected void ControlFocus(string focus)
        {
            switch (_currentWindow)
            {
                case Enums.GUIWindow.Dictionary:
                    if (!LocalizationManager.Configuration.dictionaryClearAdd) return;
                    break;
                case Enums.GUIWindow.Language:
                    if (!LocalizationManager.Configuration.languageClearAdd) return;
                    break;
                case Enums.GUIWindow.Category:
                    if (!LocalizationManager.Configuration.categoryClearAdd) return;
                    break;
                case Enums.GUIWindow.Configuration: return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GUI.GetNameOfFocusedControl() != focus) return;
            if (Event.current is { isKey: true }) EditorGUI.FocusTextInControl(focus);
        }

        #endregion

        #region LAYOUT

        protected void OnGUI()
        {
            if(!hasFocus) return;
            if (!LocalizationManager.IsDataLoaded) return;
            WindowToolbar();
            switch (_currentWindow)
            {
                case Enums.GUIWindow.Dictionary:
                    ShowDictionaryLayout();
                    break;
                case Enums.GUIWindow.Language:
                    ShowLanguagesLayout();
                    break;
                case Enums.GUIWindow.Category:
                    ShowCategoriesLayout();
                    break;
                case Enums.GUIWindow.Configuration:
                    ShowConfigurationLayout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ShowDictionaryLayout()
        {
            _dictionaryEditor.ShowLayout();
        }

        private static void ShowLanguagesLayout()
        {
            _languagesEditor.ShowLayout();
        }

        private void ShowCategoriesLayout()
        {
            
            _categoriesEditor.ShowLayout();
        }

        private void ShowConfigurationLayout()
        {
           
            _configurationEditor.ShowLayout();
        }

        #endregion

        #region CENTER SECTION

        private void WindowToolbar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(DICTIONARY_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Dictionary;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(LANGUAGES_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Language;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CATEGORIES_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Category;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CONFIGURATION_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Configuration;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region COMMONS

        private static async void LoadData()
        {
            await LocalizationManager.Instance.Init();
            _dictionaryEditor ??= (DictionaryEditor)CreateInstance(typeof(DictionaryEditor));
            _languagesEditor ??= (LanguagesEditor)CreateInstance(typeof(LanguagesEditor));
            _categoriesEditor ??= (CategoriesEditor)CreateInstance(typeof(CategoriesEditor));
            _configurationEditor ??= (ConfigurationEditor)CreateInstance(typeof(ConfigurationEditor));
        }

        protected static GUIContent GetGUIContent(Texture t, string tooltip)
        {
            return new GUIContent(t, tooltip);
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
            Repaint();
            _currentCoroutine = null;
            AddActionRunning = false;

            onComplete?.Invoke();
        }

        public virtual void ClearAddTextField()
        {
        }

        #region GUI ELEMENTS

        protected static void ShowHeader1(string text, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(text, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label), options);
            GUILayout.Space(10);
        }

        protected static void ShowHeader2(string text, params GUILayoutOption[] options)
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

        protected static void ShowLabelPopupSelection(string label, ref string categoryValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);

            if (LocalizationManager.OrderedCategories != null) //To avoid possible errors while loading data
            {
                if (LocalizationManager.Categories.IndexOf(categoryValue) != -1) //Category value is empty when loading
                {
                    var index = EditorGUILayout.Popup(LocalizationManager.Categories.IndexOf(categoryValue), LocalizationManager.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
                    categoryValue = LocalizationManager.Categories[index];
                }
                else
                {
                    EditorGUILayout.Popup(0, LocalizationManager.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
                    if (LocalizationManager.OrderedCategories.Count > 0) categoryValue = LocalizationManager.Categories[0];
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
            //TODO:
            // var dividerRect = new Rect(width, 0f, dividerWidth, position.height);
            // EditorGUIUtility.AddCursorRect(dividerRect, MouseCursor.ResizeHorizontal);
            // EditorGUI.DrawRect(dividerRect, Color.black);
            // RedimensionEvent(dividerRect);
        }

        protected void RedimensionEvent(Rect dividerRect)
        {
            //TODO:
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

        protected static float GetWidthSize(float percent)
        {
            return percent * WindowSize.x;
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

        #endregion
    }
#endif
}
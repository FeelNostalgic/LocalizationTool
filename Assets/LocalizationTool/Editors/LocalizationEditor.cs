using System;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
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
        protected static Texture FavouriteIcon => EditorGUIUtility.IconContent("d_Favorite").image;
        protected static Texture ImportIcon => EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
        protected static Texture ExportIcon => EditorGUIUtility.IconContent("d_SaveAs").image;
        protected static Texture LockIcon => EditorGUIUtility.IconContent("d_AssemblyLock").image;
        protected static Texture GearIcon => EditorGUIUtility.IconContent("d__Popup").image;
        protected static Texture WarningIcon => EditorGUIUtility.IconContent("d_console.warnicon.sml").image;

        #endregion

        #region EDITOR VARIABLES

        private int _currentWindowToolbarIndex;

        private Vector2 _scrollRight;

        private Enums.GUIWindow _currentWindow;
        private DictionaryEditor _dictionaryEditor;
        private LanguagesEditor _languagesEditor;
        private CategoriesEditor _categoriesEditor;
        private ConfigurationEditor _configurationEditor;

        protected readonly string[] _csvSeparators = { ",", ";", ".", ":", "|", "=" };

        #endregion

        #region DIMENSION VARIABLES

        protected static readonly Vector2 _windowSize = new(1400, 1000);
        protected readonly GUILayoutOption _height = GUILayout.Height(40);

        #endregion

        [MenuItem("Tool/LocalizationEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationEditor));
            window.minSize = _windowSize;
            window.titleContent = new GUIContent("Localization Tool");
        }

        protected virtual void OnEnable()
        {
            LoadData();
        }

        protected virtual void OnDisable()
        {
            //
        }
        
        protected void OnGUI()
        {
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

        // private void Update()
        // {
        //     switch (_currentWindow)
        //     {
        //         case Enums.GUI_WINDOW.Dictionary:
        //             //TODO
        //             break;
        //         case Enums.GUI_WINDOW.Languages:
        //             // TODO
        //             break;
        //         case Enums.GUI_WINDOW.Categories:
        //             // TODO
        //             break;
        //         case Enums.GUI_WINDOW.Configuration:
        //             // TODO
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
        
        private async void LoadData()
        {
            await LocalizationManager.Instance.Init();
        }
        
        private void ShowDictionaryLayout()
        {
            _dictionaryEditor ??= (DictionaryEditor)CreateInstance(typeof(DictionaryEditor));
            _dictionaryEditor.ShowLayout();
        }

        private void ShowLanguagesLayout()
        {
            _languagesEditor ??= (LanguagesEditor)CreateInstance((typeof(LanguagesEditor)));
            _languagesEditor.ShowLayout();
        }

        private void ShowCategoriesLayout()
        {
            _categoriesEditor ??= (CategoriesEditor)CreateInstance(typeof(CategoriesEditor));
            _categoriesEditor.ShowLayout();
        }

        private void ShowConfigurationLayout()
        {
            _configurationEditor ??= (ConfigurationEditor)CreateInstance(typeof(ConfigurationEditor));
            _configurationEditor.ShowLayout();
        }

        #region CENTER SECTION

        private void WindowToolbar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Dictionary"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Dictionary;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Languages"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Language;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Categories"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Category;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Configuration"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUIWindow.Configuration;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region COMMONS

        protected GUIContent GetGUIContent(Texture t, string tooltip)
        {
            return new GUIContent(t, tooltip);
        }
        
        #region GUI ELEMENTS

        protected void ShowHeader1(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(name, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label), options);
            GUILayout.Space(10);
        }

        protected void ShowHeader2(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(name, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label), options);
            GUILayout.Space(10);
        }

        protected void ShowExportImportSubHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, CustomStyles.GetStyle(Enums.CustomStyleName.Header2LowerCenter14Label), options);
            GUILayout.Space(10);
        }

        protected void ShowSubHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleLeft15Label), options);
            GUILayout.Space(10);
        }

        protected void ShowSectionHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter15Label), options);
            GUILayout.Space(10);
        }

        protected void ShowLabelPopupSelection(string label, ref string categoryValue)
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

        protected void ShowLabelTextFieldVertical(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUI.SetNextControlName(label);
            GUILayout.Label(label, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);
            keyValue = EditorGUILayout.TextField(keyValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));
            GUILayout.EndVertical();
        }

        protected void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, CustomStyles.GetStyle(Enums.CustomStyleName.FeedbackLabel));
        }

        internal virtual void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            Repaint();
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

        protected float GetWidthSize(float percent)
        {
            return percent * _windowSize.x;
        }

        protected GUILayoutOption MinWidthOption(float width)
        {
            return GUILayout.MinWidth(width);
        }

        protected GUILayoutOption MaxWidthOption(float width)
        {
            return GUILayout.MaxWidth(width);
        }

        protected GUILayoutOption MinHeightOption(float height)
        {
            return GUILayout.MinHeight(height);
        }

        protected GUILayoutOption MaxHeightOption(float height)
        {
            return GUILayout.MaxHeight(height);
        }

        #endregion
        
        #endregion
    }
#endif
}
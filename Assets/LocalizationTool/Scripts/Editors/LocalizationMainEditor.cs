using System;
using System.Collections;
using LocalizationTool.Data;
using LocalizationTool.Scripts.General;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
{
#if UNITY_EDITOR
    public class LocalizationMainEditor : EditorWindow
    {
        #region PUBLIC VARIABLES

        public static LocalizationMainEditor Instance => _instance;

        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationMainEditor _instance;

        #endregion
        
        #region PROTECTED VARIABLES
        protected readonly string[] CsvSeparators = { ";", ",", ".", ":", "|", "=" };

        #endregion

        #region EDITOR VARIABLES

        private int _currentWindowToolbarIndex;

        private Vector2 _scrollRight;

        private Enums.GUIWindow _currentWindow;
        // private static DictionaryEditor _dictionaryEditor;
        // private static LanguagesEditor _languagesEditor;
        private static CategoriesEditor _categoriesEditor;
        // private static ConfigurationEditor _configurationEditor;
        
        #endregion

        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(1400, 1000);

        #endregion

        [MenuItem("Tools/LocalizationTool/Manager", false, -30)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationMainEditor));
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
            _instance ??= this;
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
            // _dictionaryEditor.ShowLayout();
        }
        
        private static void ShowLanguagesLayout()
        {
            // _languagesEditor.ShowLayout();
        }
        
        private void ShowCategoriesLayout()
        {
            
            _categoriesEditor.ShowLayout();
        }
        
        private void ShowConfigurationLayout()
        {
           
            // _configurationEditor.ShowLayout();
        }
        
        public void ControlFocus(string focus)
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
            // _dictionaryEditor ??= (DictionaryEditor)CreateInstance(typeof(DictionaryEditor));
            // _languagesEditor ??= (LanguagesEditor)CreateInstance(typeof(LanguagesEditor));
            _categoriesEditor ??= new CategoriesEditor();
            // _configurationEditor ??= (ConfigurationEditor)CreateInstance(typeof(ConfigurationEditor));
        }
        
        #region GUI ELEMENTS
        
        #endregion

        #endregion
    }
#endif
}
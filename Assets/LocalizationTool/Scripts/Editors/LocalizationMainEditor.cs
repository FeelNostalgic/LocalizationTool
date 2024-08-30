using System;
using System.Collections;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
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
        
        #region EDITOR VARIABLES

        private int _currentWindowToolbarIndex;

        private Vector2 _scrollRight;

        //private Enums.GUIWindow _currentWindow;
        private static EditorWindowAbstract _currentEditorWindow;
        private static EditorWindowAbstract _dictionaryEditor;
        private static EditorWindowAbstract _languagesEditor;
        private static EditorWindowAbstract _categoriesEditor;
        private static EditorWindowAbstract _configurationEditor;
        
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
            _instance ??= this;
            _currentEditorWindow?.OnEnable();
            LoadWindows();
        }

        // Called when unity editor is closed
        protected virtual void OnDisable()
        {
            _currentEditorWindow?.OnDisable();
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
            _currentEditorWindow?.ShowLayout();
        }

        private static void LoadWindows()
        {
            _dictionaryEditor ??= new DictionaryEditor();
            _languagesEditor ??= new LanguagesEditor();
            _categoriesEditor ??= new CategoriesWindow();
            _configurationEditor ??= new ConfigurationEditor();
            _currentEditorWindow = _dictionaryEditor;
        }
        
        #endregion

        #region CENTER SECTION

        private static void WindowToolbar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(DICTIONARY_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _dictionaryEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(LANGUAGES_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _languagesEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CATEGORIES_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _categoriesEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CONFIGURATION_TOOLBAR_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _configurationEditor;
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
        }
        
        #region GUI ELEMENTS
        
        #endregion

        #endregion
    }
#endif
}
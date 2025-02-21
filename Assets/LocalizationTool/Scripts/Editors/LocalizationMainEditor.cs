#if UNITY_EDITOR

using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
{
    public class LocalizationMainEditor : EditorWindow
    {
        #region PUBLIC VARIABLES

        public static LocalizationMainEditor Instance { get; private set; }

        #endregion

        #region PRIVATE VARIABLES

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
        public Rect Position => position;

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

        protected void OnEnable()
        {
            Instance ??= this;
            _currentEditorWindow?.OnEnable();
            LoadWindows();
        }

        // Called when unity editor is closed
        protected void OnDisable()
        {
            _currentEditorWindow?.OnDisable();
        }

        // Called when editor is closed
        protected void OnDestroy()
        {
            //
        }

        //TODO: revisar cuando se repinta la interfaz
        protected void RepaintGUI()
        {
            Repaint();
        }

        #endregion

        #region LAYOUT

        protected void OnGUI()
        {
            if (!hasFocus) return;
            if (!CacheDataSO.IsDataLoaded) return;
            WindowToolbar();
            _currentEditorWindow?.ShowLayout();
        }

        private static void LoadWindows()
        {
            _dictionaryEditor ??= new DictionaryEditor();
            _languagesEditor ??= new LanguagesEditor();
            _categoriesEditor ??= new CategoriesEditor();
            _configurationEditor ??= new ConfigurationEditor();
            if (_currentEditorWindow.IsNull()) _currentEditorWindow = _dictionaryEditor;
            
            LocalizationManager.Instance.InitForEditor();
        }

        #endregion

        #region CENTER SECTION

        private static void WindowToolbar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(DICTIONARY_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _dictionaryEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(LANGUAGES_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _languagesEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CATEGORIES_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _categoriesEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(CONFIGURATION_LABEL), EditorStyles.toolbarButton))
            {
                _currentEditorWindow = _configurationEditor;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region DATABASE

        private static void LoadData()
        {
            CacheDataSO.InitForEditor();
        }

        #endregion
    }
}
#endif
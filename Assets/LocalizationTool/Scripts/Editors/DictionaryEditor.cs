using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Data;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
{
#if UNITY_EDITOR
    public class DictionaryEditor : EditorWindowAbstract
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _searchKeyValue = "";
        private int _searchCategoryIndex = 0;

        private string _addKeyValue;
        private string _addCategoryValue;

        private Vector2 _scrollCenter;
        private Vector2 _scrollToolbar;
        private int _currentLanguageToolbarIndex;

        private readonly Dictionary<string, string> _currentKeyValueDictionary = new();
        private readonly Dictionary<string, string> _currentKeyGroupDictionary = new();
        private readonly Dictionary<string, Vector2> _currentKeyScrollPosition = new();

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.225f;
        

        // FUTURE
        // private float _dividerPosition = 305f;
        // private const float dividerWidth = 5f;
        // private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        protected override void OnEnable()
        {
            LocalizationManager.Instance.OnDefaultCategoryUpdate += OnDefaultCategoryUpdate;
        }

        protected override void OnDisable()
        {
            LocalizationManager.Instance.OnDefaultCategoryUpdate -= OnDefaultCategoryUpdate;
        }

        public void ShowLayout()
        {
            ControlFocus(KEY_LABEL_UPPER);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal();
            ShowLeftSection();

            ShowVerticalLine(5);
            // TODO: VerticalReDimensionalDivisionLine(_leftSectionWidth);

            ShowCenterSection();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        #endregion

        #region PRRIVATE METHODS

        #region LEFT SECTION

        private void ShowLeftSection()
        {
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT, WindowSize.x)));
            GUILayout.FlexibleSpace();
            ShowAddSection();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ShowAddSection()
        {
            try
            {
                ShowHorizontalLine(5);
                GUILayout.BeginVertical("box");
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(70));
                ShowLabelTextFieldVertical(KEY_LABEL_UPPER, ref _addKeyValue);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Height(70));
                ShowLabelPopupSelection(CATEGORY_LABEL_UPPER, ref _addCategoryValue);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(312), GUILayout.Height(70));

                if (GUILayout.Button(new GUIContent(AddIcon, ADD_KEY_BUTTON_TOOLTIP), GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    LocalizationManager.Instance.AddNewKey(_addKeyValue, _addCategoryValue, this);
                }

                if (GUI.GetNameOfFocusedControl() == KEY_LABEL_UPPER)
                {
                    if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                    {
                        if (!AddActionRunning) LocalizationManager.Instance.AddNewKey(_addKeyValue, _addCategoryValue, this);
                    }
                }

                GUILayout.Space(5);
                ShowTextAreaFeedback(FeedbackLabel);
                GUILayout.EndVertical();

                GUILayout.EndVertical();
                ShowHorizontalLine(5);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //Ignore
            }
        }

        #endregion

        #region CENTER SECTION

        private void ShowCenterSection()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);

            LanguageToolBarSection();

            ShowHorizontalLine(5);

            GUILayout.Space(5);

            TitleCenterSection();

            GUILayout.Space(5);

            ShowHorizontalLine(5);

            GUILayout.Space(10); // Vertical Space

            SearchBarCenterSection();

            GUILayout.Space(20);

            ColumnsTitleCenterSection();

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void LanguageToolBarSection()
        {
            try
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                _scrollToolbar = EditorGUILayout.BeginScrollView(_scrollToolbar, GUILayout.Height(40));
                var toolbarItems = LocalizationManager.ActiveLanguages.Select(t => t).ToArray();
                if (toolbarItems.Length != 0)
                {
                    _currentLanguageToolbarIndex = GUILayout.Toolbar(LocalizationManager.Instance.CurrentToolbarLanguageIndex, toolbarItems);
                    if (LocalizationManager.Instance.CurrentLanguageInDictionarySection != LocalizationManager.ActiveLanguages[_currentLanguageToolbarIndex])
                    {
                        LocalizationManager.Instance.CurrentLanguageInDictionarySection = LocalizationManager.ActiveLanguages[_currentLanguageToolbarIndex];
                        GUI.FocusControl(null);
                    }
                }

                EditorGUILayout.EndScrollView();

                GUILayout.EndHorizontal();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //Ignore
            }
        }

        private static void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(RefreshIcon, RELOAD_BUTTON_TOOLTIP), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.Instance.RefreshDictionaryData();
                LocalizationManager.Log(DICTIONARY_LOADED_LOG);
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(LocalizationManager.Instance.CurrentLanguageInDictionarySection, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void SearchBarCenterSection()
        {
            GUILayout.BeginHorizontal();

            if (LocalizationManager.OrderedCategories != null)
            {
                if (_searchCategoryIndex >= LocalizationManager.OrderedCategories.Count) _searchCategoryIndex = 0;
                GUI.SetNextControlName("Popup");
                _searchCategoryIndex = EditorGUILayout.Popup(_searchCategoryIndex, LocalizationManager.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup), GUILayout.Width(200));
            }

            GUILayout.Space(5);

            GUI.SetNextControlName("SEARCH");
            _searchKeyValue = EditorGUILayout.TextField(_searchKeyValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));
            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private static void ColumnsTitleCenterSection()
        {
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Colors.DEEP_GRAY : Colors.DEEP_GRAY_A;
            GUILayout.BeginHorizontal();
            // Column 1
            GUILayout.BeginVertical("box");
            GUILayout.Label(KEY_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel), GUILayout.Width(275));
            GUILayout.EndVertical();

            // Column 2
            GUILayout.BeginVertical("box");
            GUILayout.Label(CATEGORY_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel), GUILayout.Width(175));
            GUILayout.EndVertical();

            // Column 3
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            GUILayout.Label(VALUE_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.backgroundColor = Colors.DEFAULT;
        }

        private void GenerateCenterScrollViewContent()
        {
            try
            {
                _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
                var filteredDicToIterate = GetFilteredDictionary();

                foreach (var keyData in filteredDicToIterate)
                {
                    UnitCenterScrollViewContent(keyData.Key, keyData.Value.Category, keyData.Value.LanguagesData[LocalizationManager.Instance.CurrentLanguageInDictionarySection]);
                }

                EditorGUILayout.EndScrollView();
            }
            catch
            {
                // ignored
            }
        }

        private Dictionary<string, KeyData> GetFilteredDictionary()
        {
            var filteredDicToIterate = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

            switch (LocalizationManager.Configuration.searchTypeIndex)
            {
                case 0: // By Key
                    if (!_searchKeyValue.IsEmpty())
                        filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture))
                            .ToDictionary(kv => (kv.Key), kv => kv.Value);
                    break;
                case 1: // By Value
                    if (!_searchKeyValue.IsEmpty())
                        filteredDicToIterate = filteredDicToIterate.Where(pair => LocalizationToolAPI.GetValueByKey(pair.Key, out _).Contains(_searchKeyValue, StringComparison.InvariantCulture))
                            .ToDictionary(kv => (kv.Key), kv => kv.Value);
                    break;
                case 2: //By Both
                    if (!_searchKeyValue.IsEmpty())
                        filteredDicToIterate = filteredDicToIterate.Where(pair =>
                                pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture)
                                || LocalizationToolAPI.GetValueByKey(pair.Key, out _).Contains(_searchKeyValue, StringComparison.InvariantCulture))
                            .ToDictionary(kv => (kv.Key), kv => kv.Value);
                    break;
            }

            if (_searchCategoryIndex != 0)
                filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Value.Category == LocalizationManager.Categories[_searchCategoryIndex])
                    .ToDictionary(kv => (kv.Key), kv => kv.Value);

            return filteredDicToIterate;
        }

        private void UnitCenterScrollViewContent(string key, string category, string value)
        {
            if (!_currentKeyValueDictionary.ContainsKey(key)) _currentKeyValueDictionary.Add(key, value);
            if (!_currentKeyGroupDictionary.ContainsKey(key)) _currentKeyGroupDictionary.Add(key, category);
            if (!_currentKeyScrollPosition.ContainsKey(key)) _currentKeyScrollPosition.Add(key, Vector2.zero);

            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginHorizontal("box", Height);
            GUI.backgroundColor = Color.clear;

            Column1(key);

            Column2(key, category);

            Column3(key, value);

            GUILayout.EndHorizontal();
        }

        private void Column1(string key)
        {
            GUILayout.BeginVertical("box", GUILayout.Width(275), Height);
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent(key, KEY_SELECTABLE_LABEL_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.KeySelectableLabel), GUILayout.Width(275), GUILayout.Height(30)))
            {
                EditorGUIUtility.systemCopyBuffer = key;
                LocalizationManager.Log(string.Format(COPY_KEY_TOOLTIP, key));
            }

            GUILayout.EndVertical();
        }

        private void Column2(string key, string category)
        {
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", Height);

            GUILayout.Space(3);
            GUILayout.BeginHorizontal(GUILayout.Width(175));
            EditorGUI.BeginChangeCheck();

            if (category.IsEmpty()) category = LocalizationManager.DefaultCategory;
            GUI.backgroundColor = Colors.DEFAULT;
            var tempCategory = EditorGUILayout.Popup(LocalizationManager.Categories.IndexOf(category), LocalizationManager.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.ScrollViewCategoryPopup));

            if (EditorGUI.EndChangeCheck())
                UpdateCategory(key, LocalizationManager.Categories[tempCategory]);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void Column3(string key, string value)
        {
            GUI.backgroundColor = Color.clear;

            GUILayout.BeginHorizontal("box", Height, GUILayout.ExpandWidth(true));

            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.BeginHorizontal(CustomStyles.GetStyle(Enums.CustomStyleName.ValueEditorPreviewBox));
            _currentKeyScrollPosition[key] = EditorGUILayout.BeginScrollView(_currentKeyScrollPosition[key], GUILayout.ExpandWidth(true), GUILayout.Height(40));
            var tempValue = EditorGUILayout.TextArea(value, CustomStyles.GetStyle(Enums.CustomStyleName.ValueEditorPreviewTextArea), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            UpdateValue(key, tempValue);
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Colors.DEFAULT;

            if (GUILayout.Button(new GUIContent(GearIcon, OPEN_TEXT_EDITOR_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                RichTextEditor.ShowWindow(tempValue, key, delegate(string s) { UpdateValue(key, s); });
                LocalizationManager.Log(string.Format(EDITING_KEY_LOG, key));
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent(DeleteIcon, string.Format(DELETE_KEY_BUTTON_TOOLTIP, key)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (LocalizationManager.Configuration.dictionaryDeleteConfirmation)
                {
                    if (EditorUtility.DisplayDialog(DELETE_DIALOG_TITLE, string.Format(DELETE_DIALOG_MESSAGE, key), DIALOG_OPTION_DELETE, DIALOG_OPTION_CANCEL))
                        DeleteKey(key);
                }
                else
                    DeleteKey(key);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

        private static void DeleteKey(string key)
        {
            LocalizationManager.Instance.RemoveKey(key);
            LocalizationManager.Log(string.Format(DELETED_KEY_LOG, key));
        }

        private void UpdateCategory(string key, string tempCategory)
        {
            if (tempCategory.Equals(_currentKeyGroupDictionary[key])) return;

            _currentKeyGroupDictionary[key] = tempCategory;
            LocalizationManager.Instance.ChangeCategory(key, _currentKeyGroupDictionary[key]);
        }

        private async void UpdateValue(string key, string tempValue)
        {
            if (tempValue.Equals(_currentKeyValueDictionary[key])) return;

            _currentKeyValueDictionary[key] = tempValue;
            await LocalizationManager.Instance.ChangeValue(key, tempValue);
        }

        private void OnDefaultCategoryUpdate(string category)
        {
            _addCategoryValue = category;
        }

        public override void ClearAddTextField()
        {
            if (!LocalizationManager.Configuration.dictionaryClearAdd) return;
            _addKeyValue = "";
            RepaintGUI();
        }

        #endregion

        #endregion
    }
#endif
}
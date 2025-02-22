#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Data;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
{
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


        // FUTURE: divider
        // private float _dividerPosition = 305f;
        // private const float dividerWidth = 5f;
        // private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public override void OnEnable()
        {
            LocalizationManager.Instance.OnDefaultCategoryUpdate += OnDefaultCategoryUpdate;
        }

        public override void OnDisable()
        {
            LocalizationManager.Instance.OnDefaultCategoryUpdate -= OnDefaultCategoryUpdate;
        }

        public override void ShowLayout()
        {
            ControlFocus(KEY_LABEL_UPPER);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal();
            ShowLeftSection();

            ShowVerticalLine(5);
            // FUTURE: VerticalReDimensionalDivisionLine(_leftSectionWidth);

            ShowCenterSection();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        protected override void ControlFocus(string focus)
        {
            if (!LocalizationManager.Configuration.dictionaryClearAdd) return;
            base.ControlFocus(focus);
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
                ShowLabelPopupSelectionCategory(CATEGORY_LABEL_UPPER, ref _addCategoryValue);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(312), GUILayout.Height(70));

                if (GUILayout.Button(new GUIContent(AddIcon, ADD_KEY_BUTTON_TOOLTIP), GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    LocalizationManager.AddNewKey(_addKeyValue, _addCategoryValue, this);
                }

                if (GUI.GetNameOfFocusedControl() == KEY_LABEL_UPPER)
                {
                    if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                    {
                        if (!AddActionRunning) LocalizationManager.AddNewKey(_addKeyValue, _addCategoryValue, this);
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
                var toolbarItems = CacheDataSO.Languages.Select(t => t).ToArray();
                if (toolbarItems.Length != 0)
                {
                    _currentLanguageToolbarIndex = GUILayout.Toolbar(LocalizationManager.CurrentToolbarLanguageIndex, toolbarItems);
                    if (LocalizationManager.CurrentLanguageInDictionarySection != CacheDataSO.Languages[_currentLanguageToolbarIndex])
                    {
                        LocalizationManager.CurrentLanguageInDictionarySection = CacheDataSO.Languages[_currentLanguageToolbarIndex];
                        GUI.FocusControl(null);
                    }
                }

                EditorGUILayout.EndScrollView();

                GUILayout.EndHorizontal();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private static void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(RefreshIcon, RELOAD_BUTTON_TOOLTIP), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.RefreshDictionaryData();
                LocalizationManager.Log(DICTIONARY_LABEL, CustomDebug.Colors.Blue, LOADED_LOG);
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(LocalizationManager.CurrentLanguageInDictionarySection, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void SearchBarCenterSection()
        {
            GUILayout.BeginHorizontal();

            if (CacheDataSO.CategoryCache != null)
            {
                if (_searchCategoryIndex >= CacheDataSO.CategoryCache.Count) _searchCategoryIndex = 0;
                GUI.SetNextControlName("Popup");
                _searchCategoryIndex = EditorGUILayout.Popup(_searchCategoryIndex, CacheDataSO.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup), GUILayout.Width(200));
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
        
        private Dictionary<string, TranslationKeyDataSO> GetFilteredDictionary()
        {
            var filteredDicToIterate = new Dictionary<string, TranslationKeyDataSO>(CacheDataSO.localizationData.LanguagesDictionary[LocalizationManager.CurrentLanguageInDictionarySection].TranslationDictionary);

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
                filteredDicToIterate = filteredDicToIterate.Where(pair => CacheDataSO.localizationData.KeysDictionary[pair.Key].category.categoryName == CacheDataSO.Categories[_searchCategoryIndex])
                    .ToDictionary(kv => (kv.Key), kv => kv.Value);

            return filteredDicToIterate;
        }

        
        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);

            const float itemHeight = 40f;
            const int visibleItemsCount = 50;
            var filteredDicToIterate = GetFilteredDictionary().ToList();

            try
            {
                if (filteredDicToIterate.Count > 0)
                {
                    GUILayout.BeginVertical();

                    var firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(_scrollCenter.y / visibleItemsCount));
                    var lastVisibleIndex = Mathf.Min(firstVisibleIndex + visibleItemsCount, filteredDicToIterate.Count);

                    GUILayout.Space(firstVisibleIndex * itemHeight);

                    for (var i = firstVisibleIndex; i < lastVisibleIndex; i++)
                    {
                        var item = filteredDicToIterate[i];
                        DrawKeyItem(item.Key, CacheDataSO.localizationData.KeysDictionary[item.Key].category.categoryName, item.Value.translationText);
                    }

                    GUILayout.Space(Mathf.Clamp((filteredDicToIterate.Count - lastVisibleIndex - 1) * itemHeight, 0f, filteredDicToIterate.Count * itemHeight)+40);
                    
                    GUILayout.EndVertical();
                }
            }
            catch (Exception e)
            {
                if (e is not ExitGUIException) Debug.LogError(e);
            }

            EditorGUILayout.EndScrollView();
        }
        
        private void DrawKeyItem(string key, string category, string value)
        {
            _currentKeyValueDictionary.TryAdd(key, value);
            _currentKeyGroupDictionary.TryAdd(key, category);
            _currentKeyScrollPosition.TryAdd(key, Vector2.zero);

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
                LocalizationManager.Log(DICTIONARY_LABEL, CustomDebug.Colors.Blue, string.Format(COPY_KEY_TOOLTIP, key));
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

            if (category.IsEmpty()) category = CacheDataSO.DefaultCategory;
            GUI.backgroundColor = Colors.DEFAULT;
            var tempCategory = EditorGUILayout.Popup(CacheDataSO.Categories.IndexOf(category), CacheDataSO.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.ScrollViewCategoryPopup));

            if (EditorGUI.EndChangeCheck())
                UpdateCategory(key, CacheDataSO.Categories[tempCategory]);

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
                LocalizationManager.Log(DICTIONARY_LABEL, CustomDebug.Colors.Blue, string.Format(EDITING_KEY_LOG, key));
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
            LocalizationManager.RemoveKey(key);
            LocalizationManager.Log(DICTIONARY_LABEL, CustomDebug.Colors.Blue, string.Format(DELETED_KEY_LOG, key));
        }

        private void UpdateCategory(string key, string tempCategory)
        {
            if (tempCategory.Equals(_currentKeyGroupDictionary[key])) return;

            _currentKeyGroupDictionary[key] = tempCategory;
            LocalizationManager.ChangeCategory(key, _currentKeyGroupDictionary[key]);
        }

        private void UpdateValue(string key, string tempValue)
        {
            if (tempValue.IsNull() || tempValue.Equals(_currentKeyValueDictionary[key])) return;

            _currentKeyValueDictionary[key] = tempValue;
            LocalizationManager.ChangeValue(key, tempValue);
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
}
#endif
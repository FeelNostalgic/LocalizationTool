using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editor
{
    public class DictionaryEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        public string AddValueFeedbackLabelText
        {
            set => _addValueFeedbackLabelText = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES
        
        private string _searchKeyValue = "";
        private Enums.GROUPS _searchGroupValue;

        private string _addKeyValue;
        private Enums.GROUPS _addGroupValue;
        private string _addValueFeedbackLabelText = "";

        private Vector2 _scrollCenter;
        private Vector2 _scrollToolbar;
        private int _currentLanguageToolbarIndex;

        private readonly Dictionary<string, string> _currentKeyValueDictionary = new();
        private readonly Dictionary<string, Enums.GROUPS> _currentKeyGroupDictionary = new();

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.225f;

        private float _dividerPosition = 305f;
        private const float dividerWidth = 5f;

        private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public void ShowLayout()
        {
            GUILayout.BeginVertical(MinHeightOption(_windowSize.y));

            //GUILayout.Space(10);
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
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));
            GUILayout.FlexibleSpace();
            ShowAddSection();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ShowAddSection()
        {
            ShowHorizontalLine(5);

            GUILayout.Space(10);
            ShowLabelTextFieldVertical("KEY", ref _addKeyValue);

            GUILayout.Space(20);

            ShowLabelEnumPopupSelection("CATEGORY", ref _addGroupValue);

            GUILayout.Space(20);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ol_plus", "Add new language"), GUILayout.MaxWidth(500), GUILayout.MaxHeight(25)))
            {
                LocalizationManager.Instance.AddNewKey(_addKeyValue, _addGroupValue, this);
                ControlTextAreaFeedbackDuration(1f);
            }

            GUILayout.Space(10);

            ShowTextAreaFeedback(_addValueFeedbackLabelText);

            GUILayout.Space(10);
            ShowHorizontalLine(5);
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

            TableTitleCenterSection();

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void SearchBarCenterSection()
        {
            GUILayout.BeginHorizontal();

            _searchGroupValue = (Enums.GROUPS)EditorGUILayout.EnumPopup(_searchGroupValue, AddGroupStyle(),GUILayout.Width(200));

            GUILayout.Space(5);

            _searchKeyValue = EditorGUILayout.TextField(_searchKeyValue, AddTextFieldStyle());
            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh", "Refresh data loading"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.Instance.RefreshDictionaryData();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(LocalizationManager.Instance.CurrentLanguageInDictionarySection, HeaderStyle());

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void TableTitleCenterSection()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("KEY", SubSectionHeaderStyle());
            GUILayout.Label("CATEGORY", SubSectionHeaderStyle(), GUILayout.MaxWidth(250));
            GUILayout.Label("VALUE", SubSectionHeaderStyle());
            GUILayout.Space(60);

            GUILayout.EndHorizontal();
        }

        private void LanguageToolBarSection()
        {
            GUILayout.BeginHorizontal();
            _scrollToolbar = EditorGUILayout.BeginScrollView(_scrollToolbar, GUILayout.Height(40));
            var toolbarItems = LocalizationManager.ActiveLanguages.Select(t => t).ToArray();
            if (toolbarItems.Length != 0)
            {
                _currentLanguageToolbarIndex = GUILayout.Toolbar(LocalizationManager.Instance.CurrentToolbarLanguageIndex, toolbarItems);
                LocalizationManager.Instance.CurrentLanguageInDictionarySection = LocalizationManager.ActiveLanguages[_currentLanguageToolbarIndex];
            }
            
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                var filteredDicToIterate = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

                if (!_searchKeyValue.Equals(""))
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture))
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);

                if (_searchGroupValue != Enums.GROUPS.None)
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Value.Category == _searchGroupValue)
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);

                foreach (var keyData in filteredDicToIterate)
                {
                    UnitCenterScrollViewContent(keyData.Key, keyData.Value.Category, keyData.Value.LanguagesData[LocalizationManager.Instance.CurrentLanguageInDictionarySection]);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(string key, Enums.GROUPS group, string value)
        {
            if (!_currentKeyValueDictionary.ContainsKey(key)) _currentKeyValueDictionary.Add(key, value);
            if (!_currentKeyGroupDictionary.ContainsKey(key)) _currentKeyGroupDictionary.Add(key, group);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.SelectableLabel(key, KeyLabelStyle(), MaxHeightOption(24));

            GUILayout.Space(10);
            var tempGroup = (Enums.GROUPS)EditorGUILayout.EnumPopup(_currentKeyGroupDictionary[key], GroupSelectionStyle(), GUILayout.MaxWidth(250));
            UpdateGroup(key, tempGroup);

            GUILayout.Space(10);
            var tempValue = EditorGUILayout.TextField(value, TextFieldValueStyle(), MinHeightOption(24));
            UpdateValue(key, group, tempValue);

            GUILayout.Space(10);

            if (GUILayout.Button(EditorGUIUtility.IconContent("Customized", "Edit"), CenterButtonComponentsStyle()))
            {
                //TODO: show interface
                Debug.Log($"{key} edited");
            }

            GUILayout.Space(10);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete"), CenterButtonComponentsStyle()))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {key}?", "Delete", "Cancel"))
                {
                    LocalizationManager.Instance.RemoveKey(key);
                    Debug.Log($"Key '{key}' removed");
                }
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

        private void UpdateGroup(string key, Enums.GROUPS tempGroup)
        {
            if (tempGroup.Equals(_currentKeyGroupDictionary[key])) return;

            _currentKeyGroupDictionary[key] = tempGroup;
            LocalizationManager.Instance.ChangeCategory(key, _currentKeyGroupDictionary[key]);
        }

        private void UpdateValue(string key, Enums.GROUPS group, string tempValue)
        {
            if (tempValue.Equals(_currentKeyValueDictionary[key])) return;

            _currentKeyValueDictionary[key] = tempValue;
            LocalizationManager.Instance.ChangeValue(key, tempValue, LocalizationManager.Instance.CurrentLanguageInDictionarySection);
            //Debug.Log($"{key} has now value: {tempValue}");
        }

        #endregion

        internal override async void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            var millisecondsDelay = (int)(durationInSeconds * 1000);
            await Task.Delay(millisecondsDelay);
            _addValueFeedbackLabelText = "";
        }

        #endregion
    }
}
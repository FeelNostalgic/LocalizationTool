using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Commons;
using LocalizationTool.Controller;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    public class DictionaryEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        public string AddValueFeedbackLabelText
        {
            get => _addValueFeedbackLabelText;
            set => _addValueFeedbackLabelText = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _searchKeyValue = "";
        private int _searchCategoryIndex = 0;

        private string _addKeyValue;
        private string _addCategoryValue;
        private string _addValueFeedbackLabelText = "";

        private Vector2 _scrollCenter;
        private Vector2 _scrollToolbar;
        private int _currentLanguageToolbarIndex;

        private readonly Dictionary<string, string> _currentKeyValueDictionary = new();
        private readonly Dictionary<string, string> _currentKeyGroupDictionary = new();
        private readonly Dictionary<string, Vector2> _currentKeyScrollPosition = new();

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.225f;

        private float _dividerPosition = 305f;
        private const float dividerWidth = 5f;

        private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationManager.Instance.OnDefaultCategoryUpdate += OnDefaultCategoryUpdate;
        }

        protected override void OnDisable()
        {
            LocalizationManager.Instance.OnDefaultCategoryUpdate -= OnDefaultCategoryUpdate;
        }

        public void ShowLayout()
        {
            GUILayout.BeginVertical(MinHeightOption(_windowSize.y));
            
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
            try
            {
                ShowHorizontalLine(5);

                GUILayout.Space(10);
                ShowLabelTextFieldVertical("KEY", ref _addKeyValue);

                GUILayout.Space(20);

                ShowLabelPopupSelection("CATEGORY", ref _addCategoryValue);

                GUILayout.Space(20);
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_ol_plus", "Add new language"), GUILayout.MaxWidth(500), GUILayout.MaxHeight(25)))
                {
                    LocalizationManager.Instance.AddNewKey(_addKeyValue, _addCategoryValue, this);
                    ControlTextAreaFeedbackDuration(1.5f);
                }

                if (GUI.GetNameOfFocusedControl() == "KEY")
                {
                    if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                    {
                        LocalizationManager.Instance.AddNewKey(_addKeyValue, _addCategoryValue, this);
                        ControlTextAreaFeedbackDuration(1.5f);
                    }
                }

                GUILayout.Space(10);

                ShowTextAreaFeedback(_addValueFeedbackLabelText);

                GUILayout.Space(10);
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
            
            GUI.SetNextControlName("Search");
            _searchKeyValue = EditorGUILayout.TextField(_searchKeyValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField));
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
            
            GUILayout.Label(LocalizationManager.Instance.CurrentLanguageInDictionarySection, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void ColumnsTitleCenterSection()
        {
            //GUI.backgroundColor = Colors.DEEP_GRAY_A;
            GUILayout.BeginHorizontal();
            GUILayout.Label("KEY", SubSectionHeaderStyle(325));
            
            GUILayout.Label("CATEGORY", SubSectionHeaderStyle(250));
            GUILayout.FlexibleSpace();
            GUILayout.Label("VALUE", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter14Label));
            GUILayout.FlexibleSpace();
            GUILayout.Space(100);
            GUILayout.EndHorizontal();
            
           // GUI.backgroundColor;
        }

        private void LanguageToolBarSection()
        {
            try
            {
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
                GUILayout.Space(5);

                GUILayout.EndHorizontal();
            }
            catch (Exception)
            {
                //Ignore
            }
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

            switch (LocalizationManager.Configuration.SearchTypeIndex)
            {
                case 0: // By Key
                    if (!_searchKeyValue.Equals(""))
                        filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture))
                            .ToDictionary(kv => (kv.Key), kv => kv.Value);
                    break;
                case 1: // By Value
                    if (!_searchKeyValue.Equals(""))
                        filteredDicToIterate = filteredDicToIterate.Where(pair => LocalizationToolController.Instance.GetValueByKey(pair.Key).Contains(_searchKeyValue, StringComparison.InvariantCulture))
                            .ToDictionary(kv => (kv.Key), kv => kv.Value);
                    break;
                case 2: //By Both
                    if (!_searchKeyValue.Equals(""))
                        filteredDicToIterate = filteredDicToIterate.Where(pair => 
                                pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture)
                                || LocalizationToolController.Instance.GetValueByKey(pair.Key).Contains(_searchKeyValue, StringComparison.InvariantCulture) )
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
            if(!_currentKeyScrollPosition.ContainsKey(key)) _currentKeyScrollPosition.Add(key, Vector2.zero);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            
            GUILayout.Space(10);

            if (GUILayout.Button(key, CustomStyles.GetStyle(Enums.CustomStyleName.KeyFixedHeightFixedWidthSelectableLabel)))
            {
                EditorGUIUtility.systemCopyBuffer = key;
                LocalizationManager.Log($"Key '{key}' copied to clipboard");
            }

            GUILayout.Space(10);
            if (category == "") category = LocalizationManager.DefaultCategory;
            var tempCategory = EditorGUILayout.Popup(LocalizationManager.Categories.IndexOf(category), LocalizationManager.Categories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.ScrollViewCategoryPopup), GUILayout.Width(250));
            UpdateCategory(key, LocalizationManager.Categories[tempCategory]);

            GUILayout.Space(10);
            
            _currentKeyScrollPosition[key] = EditorGUILayout.BeginScrollView(_currentKeyScrollPosition[key], GUIStyle.none, GUIStyle.none, GUILayout.Height(28), GUILayout.ExpandWidth(true));
            var tempValue = EditorGUILayout.TextArea(value, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            UpdateValue(key, tempValue);
            
            GUILayout.Space(8);

            if (GUILayout.Button(EditorGUIUtility.IconContent("Customized", "Edit"), CustomStyles.GetStyle(Enums.CustomStyleName.CenteredButtonWithIcon)))
            {
                RichTextEditor.ShowWindow(tempValue, key, delegate(string s) { UpdateValue(key, s); });
                LocalizationManager.Log($"Editing key '{key}'");
            }

            GUILayout.Space(8);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete"), CustomStyles.GetStyle(Enums.CustomStyleName.CenteredButtonWithIcon)))
            {
                if (LocalizationManager.Configuration.DictionaryDeleteConfirmation)
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {key}?", "Delete", "Cancel"))
                    {
                        LocalizationManager.Instance.RemoveKey(key);
                        LocalizationManager.Log($"Key '{key}' removed");
                    }
                }
                else
                {
                    LocalizationManager.Instance.RemoveKey(key);
                    LocalizationManager.Log($"Key '{key}' removed");
                }

            }

            GUILayout.Space(14);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

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

        #endregion

        internal override async void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            var millisecondsDelay = (int)(durationInSeconds * 1000);
            await Task.Delay(millisecondsDelay);
            _addValueFeedbackLabelText = "";
            base.ControlTextAreaFeedbackDuration(durationInSeconds);
        }
        
        #endregion
    }
#endif
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editor
{
    public class LocalizationEditor : EditorWindow
    {
        #region EDITOR VARIABLES

        private string _searchKeyValue = "";
        private Data.GROUPS _searchGroupValue;

        private string _addKeyValue;
        private Data.GROUPS _addGroupValue;
        private string _addValueFeedbackLabelText = "";

        private string _removeKeyValue;
        private string _removeFeedbackLabelText = "";

        private static  Data.LANGUAGES _currentLanguage;

        private Vector2 _scrollCenter;
        private Vector2 _scrollRight;
        private Vector2 _scrollToolbar;
        private int _currentLanguageToolbarIndex;
        private int _currentWindowToolbarIndex;

        private Data.LANGUAGES _addLanguageValue;
        private string _addLanguageFeedbackLabelText = "";

        private Dictionary<string, string> _currentKeyValueDictionary = new();
        private Dictionary<string, Data.GROUPS> _currentKeyGroupDictionary = new();

        private Data.GUI_WINDOW _currentWindow;
        
        #endregion

        #region DIMENSION VARIABLES

        protected static readonly Vector2 _windowSize = new(1400, 750);
        private float _leftSectionWidthPercent = 0.225f;
        private float _rigthSectionWidthPercent = 0.225f;

        private float _dividerPosition = 305f;
        private const float dividerWidth = 5f;

        private bool isResizingDivider = false;

        #endregion

        #region PUBLIC VARIABLES

        public string AddValueFeedbackLabelText
        {
            set => _addValueFeedbackLabelText = value;
        }

        public string AddLanguageFeedbackLabelText
        {
            set => _addLanguageFeedbackLabelText = value;
        }

        #endregion

        [MenuItem("Tool/LocalizationEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationEditor));
            window.minSize = _windowSize;
            window.titleContent = new GUIContent("Localization Tool");
        }

        private void OnGUI()
        {
            LoadData();
            WindowToolbar();
            switch (_currentWindow)
            {
                case Data.GUI_WINDOW.Dictionary: ShowDictionaryLayout();
                    break;
                case Data.GUI_WINDOW.Languages: ShowLanguagesLayout();
                    break;
                case Data.GUI_WINDOW.Categories: ShowCategoriesLayout();
                    break;
                case Data.GUI_WINDOW.Configuration: ShowConfigurationLayout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // private void OnDisable()
        // {
        //     foreach (var (_, value) in _currentKeyValueDictionary)
        //     {
        //         value.Dispose();
        //     }
        // }

        private void LoadData()
        {
            LocalizationManager.Instance.Init();
            //TODO: crear selector de lenguas y poner favorita (inicial)
            //if (LocalizationManager.Instance.ActiveLanguages.Count > 0 && _currentLanguage == null) _currentLanguage = LocalizationManager.Instance.ActiveLanguages[0];
        }

        private void ShowDictionaryLayout()
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
        
        private void ShowLanguagesLayout()
        {
            
        }
        
        private void ShowCategoriesLayout()
        {
            
        }
        
        private void ShowConfigurationLayout()
        {
            
        }
        
        #region LEFT SECTION

        private void ShowLeftSection()
        {
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(_leftSectionWidthPercent)));
            //ShowSearchSection();
            ShowAddSection();
            GUILayout.EndVertical();
        }

        private void ShowAddSection()
        {
            GUILayout.BeginVertical();
            //ShowHeader("ADD");

            GUILayout.Space(10);
            ShowLabelTextField("KEY", ref _addKeyValue);

            GUILayout.Space(20);

            ShowLabelEnumPopupSelection("GROUP", ref _addGroupValue);

            GUILayout.Space(10);

            if (GUILayout.Button("ADD", ButtonStyle()))
            {
                LocalizationManager.Instance.AddNewKey(_addKeyValue, _addGroupValue, this);
                ControlTextAreaFeedbackDuration(1f, Data.GUI_SECTIONS.valueFeedback);
            }

            GUILayout.Space(10);

            ShowTextAreaFeedback(_addValueFeedbackLabelText);

            GUILayout.Space(10);
            ShowHorizontalLine(5);

            GUILayout.EndVertical();
        }
        
        #endregion

        #region CENTER SECTION

        private void ShowCenterSection()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);
            
            LanguageToolBarSection();

            GUILayout.Space(5);

            TitleCenterSection();

            GUILayout.Space(5);

            ShowHorizontalLine(5);

            GUILayout.Space(20); // Vertical Space

            SearchBarCenterSection();

            GUILayout.Space(10);
            
            TableTitleCenterSection();

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void SearchBarCenterSection()
        {
            GUILayout.BeginHorizontal();

            _searchGroupValue = (Data.GROUPS) EditorGUILayout.EnumPopup(_searchGroupValue, GUILayout.Width(200));

            GUILayout.Space(5);

            _searchKeyValue = EditorGUILayout.TextField(_searchKeyValue);
            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private void WindowToolbar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Dictionary"), EditorStyles.toolbarButton))
            {
                _currentWindow = Data.GUI_WINDOW.Dictionary;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Languages"), EditorStyles.toolbarButton))
            {
                _currentWindow = Data.GUI_WINDOW.Languages;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Categories"), EditorStyles.toolbarButton))
            {
                _currentWindow = Data.GUI_WINDOW.Categories;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Configuration"), EditorStyles.toolbarButton))
            {
                _currentWindow = Data.GUI_WINDOW.Configuration;
            }
            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }
        
        private void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh", "Refresh data loading"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.Instance.RefreshData();
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label(_currentLanguage.ToString(), HeaderStyle());
            
            GUILayout.FlexibleSpace();
            
            GUILayout.EndHorizontal();
        }

        
        private void TableTitleCenterSection()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("KEY", SubSectionHeaderStyle());
            GUILayout.Label("GROUP", SubSectionHeaderStyle(), GUILayout.MaxWidth(250));
            GUILayout.Label("VALUE", SubSectionHeaderStyle());
            GUILayout.Space(60);

            GUILayout.EndHorizontal();
        }


        private void LanguageToolBarSection()
        {
            GUILayout.BeginHorizontal();
            _scrollToolbar = EditorGUILayout.BeginScrollView(_scrollToolbar, GUILayout.Height(40));
            var toolbarItems = LocalizationManager.Instance.ActiveLanguages.Select(t => t.ToString()).ToArray();

            _currentLanguageToolbarIndex = GUILayout.Toolbar((int)_currentLanguage, toolbarItems);
            _currentLanguage = (Data.LANGUAGES)_currentLanguageToolbarIndex;

            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                var filteredDicToIterate = new Dictionary<string, KeyData>(LocalizationManager.Instance.Dictionary);
                
                if (!_searchKeyValue.Equals(""))
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.Contains(_searchKeyValue, StringComparison.InvariantCulture))
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);
                
                if (_searchGroupValue != Data.GROUPS.None)
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Value.Group == _searchGroupValue)
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);

                foreach (var keyData in filteredDicToIterate)
                {
                    UnitCenterScrollViewContent(keyData.Key, keyData.Value.Group, keyData.Value.LanguagesData[_currentLanguage]);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(string key, Data.GROUPS group, string value)
        {
            if (!_currentKeyValueDictionary.ContainsKey(key)) _currentKeyValueDictionary.Add(key, value);
            if (!_currentKeyGroupDictionary.ContainsKey(key)) _currentKeyGroupDictionary.Add(key, group);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.SelectableLabel(key, KeyLabelStyle(), MaxHeightOption(24));

            GUILayout.Space(10);
            var tempGroup = (Data.GROUPS)EditorGUILayout.EnumPopup(_currentKeyGroupDictionary[key], GroupSelectionStyle(), GUILayout.MaxWidth(250));
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
        
        #region RIGHT SECTION

        private void ShowRightSection()
        {
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(_rigthSectionWidthPercent)));
            ShowLanguageSection();
            GUILayout.EndVertical();
        }

        private void ShowLanguageSection()
        {
            GUILayout.BeginVertical();

            ShowHeader("Languages");

            ShowHorizontalLine(5);

            //GUILayout.BeginVertical();
            // GUILayout.Space(10);
            // ShowLabelEnumPopupSelection("LANGUAGE", ref _addLanguageValue);
            //
            // GUILayout.Space(10);
            //
            // if (GUILayout.Button("Add new language", ButtonStyle()))
            // {
            //     LocalizationManager.Instance.AddNewLanguageToCSV(_addLanguageValue, this);
            //     ControlTextAreaFeedbackDuration(0.75f, Data.GUI_SECTIONS.languageFeedback);
            // }

            //GUILayout.Space(10);

            //ShowTextAreaFeedback(_addLanguageFeedbackLabelText);

            //GUILayout.EndVertical();

            //ShowHorizontalLine(5);

            GenerateLanguageSelectorScrollViewContent();

            GUILayout.EndVertical();
        }

        private void GenerateLanguageSelectorScrollViewContent()
        {
            _scrollRight = EditorGUILayout.BeginScrollView(_scrollRight);
            foreach (var t in LocalizationManager.Instance.ActiveLanguages)
            {
                LanguageSelectorScrollViewContent(t.ToString());
            }

            EditorGUILayout.EndScrollView();
        }

        private void LanguageSelectorScrollViewContent(string language)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(language))
            {
                //_currentLanguage = language;
            }

            // if (GUILayout.Button("R", GUILayout.MaxWidth(65)))
            // {
            //     if (Enum.TryParse(language, out Data.LANGUAGES languageToRemove)) LocalizationManager.Instance.RemoveLanguageFromCSV(languageToRemove);
            // }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

        private void UpdateGroup(string key, Data.GROUPS tempGroup)
        {
            if (tempGroup.Equals(_currentKeyGroupDictionary[key])) return;

            _currentKeyGroupDictionary[key] = tempGroup;
            LocalizationManager.Instance.ChangeGroup(key, _currentKeyGroupDictionary[key]);
        }

        private void UpdateValue(string key, Data.GROUPS group, string tempValue)
        {
            if (tempValue.Equals(_currentKeyValueDictionary[key])) return;
            
            _currentKeyValueDictionary[key] = tempValue;
            LocalizationManager.Instance.ChangeValue(key, tempValue, _currentLanguage);
            //Debug.Log($"{key} has now value: {tempValue}");
        }

        #endregion
        
        #region COMMONS

        protected void ShowHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(name, HeaderStyle(), options);
            GUILayout.Space(10);
        }

        protected void ShowLabelEnumPopupSelection<T>(string label, ref T groupValue) where T : Enum
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            groupValue = (T)EditorGUILayout.EnumPopup(groupValue);
            GUILayout.EndVertical();
        }

        protected void ShowLabelTextField(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            GUILayout.Space(4);
            keyValue = EditorGUILayout.TextField(keyValue);
            GUILayout.EndVertical();
        }

        protected void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, FeedbackLabelStyle());
        }

        protected async void ControlTextAreaFeedbackDuration(float durationInSeconds, Data.GUI_SECTIONS feedback)
        {
            var millisecondsDelay = (int)(durationInSeconds * 1000);
            await Task.Delay(millisecondsDelay);
            switch (feedback)
            {
                case Data.GUI_SECTIONS.valueFeedback:
                    _addValueFeedbackLabelText = "";
                    break;
                case Data.GUI_SECTIONS.removeFeedback:
                    break;
                case Data.GUI_SECTIONS.languageFeedback:
                    _addLanguageFeedbackLabelText = "";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(feedback), feedback, null);
            }
        }

        protected static void ShowVerticalLine(float width)
        {
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(width));
        }

        protected static void ShowHorizontalLine(float height)
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
        }

        protected void VerticalReDimensionalDivisionLine(float width)
        {
            var dividerRect = new Rect(width, 0f, dividerWidth, position.height);
            EditorGUIUtility.AddCursorRect(dividerRect, MouseCursor.ResizeHorizontal);
            EditorGUI.DrawRect(dividerRect, Color.black);
            RedimensionEvent(dividerRect);
        }

        protected void RedimensionEvent(Rect dividerRect)
        {
            isResizingDivider = Event.current.type switch
            {
                EventType.MouseDown when dividerRect.Contains(Event.current.mousePosition) => true,
                EventType.MouseUp => false,
                _ => isResizingDivider
            };

            if (!isResizingDivider) return;

            _dividerPosition = Event.current.mousePosition.x;
            Repaint();
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

        #region Styles

        protected GUIStyle CenterButtonComponentsStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 25,
                fixedWidth = 25
            };

            return style;
        }
        
        protected GUIStyle FixedWidthStyle(float width)
        {
            var style = new GUIStyle
            {
                fixedWidth = width
            };

            return style;
        }

        protected GUIStyle SubSectionHeaderStyle()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle FeedbackLabelStyle()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle ButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                fixedHeight = 22,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle HeaderStyle()
        {
            var style = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle KeyLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle TextFieldValueStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle GroupSelectionStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 24,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        #endregion

        #endregion
    }
}
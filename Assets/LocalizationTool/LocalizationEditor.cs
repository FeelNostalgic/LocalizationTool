using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UniRx;

namespace LocalizationTool.Editor
{
    public class LocalizationEditor : EditorWindow
    {
        #region EDITOR VARIABLES

        private string _searchKeyValue;
        private Data.GROUPS _searchGroupValue;

        private string _addKeyValue;
        private Data.GROUPS _addGroupValue;
        private string _addValueFeedbackLabelText = "";

        private string _removeKeyValue;
        private string _removeFeedbackLabelText = "";

        private static string _currentLanguage;

        private Vector2 _scrollCenter;
        private Vector2 _scrollRight;

        private Data.LANGUAGES _addLanguageValue;
        private string _addLanguageFeedbackLabelText = "";

        private Dictionary<string, string> _currentKeyValueDictionary = new();
        private Dictionary<string, Data.GROUPS> _currentKeyGroupDictionary = new();

        #endregion

        #region DIMENSION VARIABLES

        private static readonly Vector2 _windowSize = new(1400, 750);
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
            ShowLayout();
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
            if (LocalizationManager.Instance.ActiveLanguages.Count > 0 && _currentLanguage == null) _currentLanguage = LocalizationManager.Instance.ActiveLanguages[0].ToString();
        }

        private void ShowLayout()
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(_windowSize.y));
            ShowLeftSection();

            ShowVerticalLine(5);
            // TODO: VerticalReDimensionalDivisionLine(_leftSectionWidth);

            ShowCenterSection();

            ShowVerticalLine(5);

            ShowRightSection();
            GUILayout.EndHorizontal();
        }

        #region LEFT SECTION

        private void ShowLeftSection()
        {
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(_leftSectionWidthPercent)));
            ShowSearchSection();
            ShowAddSection();
            //ShowRemoveSection();
            GUILayout.EndVertical();
        }

        private void ShowSearchSection()
        {
            GUILayout.BeginVertical();
            ShowHeader("SEARCH");

            ShowLabelTextField("KEY", ref _searchKeyValue);

            GUILayout.Space(20);

            ShowLabelEnumPopupSelection("GROUP", ref _searchGroupValue);

            GUILayout.Space(10);
            ShowHorizontalLine(5);

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
                if (Enum.TryParse(_currentLanguage, out Data.LANGUAGES language)) LocalizationManager.Instance.AddNewKeyValue(_addKeyValue, _addGroupValue, "", this);
                ControlTextAreaFeedbackDuration(1f, Data.GUI_SECTIONS.valueFeedback);
            }

            GUILayout.Space(10);

            ShowTextAreaFeedback(_addValueFeedbackLabelText);

            GUILayout.Space(10);
            ShowHorizontalLine(5);

            GUILayout.EndVertical();
        }

        private void ShowRemoveSection()
        {
            //TODO: quitar seccion y poner boton en seccion central con popup para confirmar
            GUILayout.BeginVertical();
            //ShowHeader("REMOVE");

            GUILayout.Space(10);
            ShowLabelTextField("KEY", ref _removeKeyValue);

            GUILayout.Space(10);

            if (GUILayout.Button("REMOVE", ButtonStyle()))
            {
                //TODO: 
            }

            GUILayout.Space(10);
            ShowTextAreaFeedback(_removeFeedbackLabelText);

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
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            if (GUILayout.Button(new GUIContent() { text = "Refresh", tooltip = "Refresh data loading from CSV" }, GUILayout.MaxWidth(65)))
            {
                LocalizationManager.Instance.RefreshData();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            ShowHeader(_currentLanguage);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            ShowHorizontalLine(5);

            GUILayout.Space(20); // Vertical Space

            GUILayout.BeginHorizontal();
            
            GUILayout.Label("KEY", SubSectionHeaderStyle());
            GUILayout.Label("GROUP", SubSectionHeaderStyle(), GUILayout.MaxWidth(250));
            GUILayout.Label("VALUE", SubSectionHeaderStyle());
            GUILayout.Space(60);

            GUILayout.EndHorizontal();

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                var filteredDicToIterate = new Dictionary<(string key, Data.GROUPS group), Dictionary<Data.LANGUAGES, string>>(LocalizationManager.Instance.Dictionary);
                if (!_searchKeyValue.Equals(""))
                {
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.key.Contains(_searchKeyValue, StringComparison.InvariantCulture))
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);
                }

                if (_searchGroupValue != Data.GROUPS.None)
                    filteredDicToIterate = filteredDicToIterate.Where(pair => pair.Key.group == _searchGroupValue)
                        .ToDictionary(kv => (kv.Key), kv => kv.Value);
                
                foreach (var ((key, group), interDic) in filteredDicToIterate)
                {
                    if (Enum.TryParse(_currentLanguage, out Data.LANGUAGES language)) UnitCenterScrollViewContent(key, group, interDic[language]);
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

            var tempValue = EditorGUILayout.DelayedTextField(value, TextFieldValueStyle(), MinHeightOption(24));
            UpdateValue(key, group, tempValue);

            GUILayout.Space(10);
            
            if (GUILayout.Button("E", ButtonStyle()))
            {
                //TODO: show interface
                Debug.Log($"{key} edited");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("R", ButtonStyle()))
            {
                //TODO
                Debug.Log($"{key} removed");
                //TODO: add confirmation popup
            }

            GUILayout.Space(10);
            
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
            if (!Enum.TryParse(_currentLanguage, out Data.LANGUAGES language)) return;

            _currentKeyValueDictionary[key] = tempValue;
            LocalizationManager.Instance.ChangeValue(key, group, tempValue, language);
            //Debug.Log($"{key} has now value: {tempValue}");
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

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            ShowLabelEnumPopupSelection("LANGUAGE", ref _addLanguageValue);

            GUILayout.Space(10);

            if (GUILayout.Button("Add new language", ButtonStyle()))
            {
                LocalizationManager.Instance.AddNewLanguageToCSV(_addLanguageValue, this);
                ControlTextAreaFeedbackDuration(0.75f, Data.GUI_SECTIONS.languageFeedback);
            }

            GUILayout.Space(10);

            ShowTextAreaFeedback(_addLanguageFeedbackLabelText);

            GUILayout.EndVertical();

            ShowHorizontalLine(5);

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
                //TODO: Load current dictionary to center section
                _currentLanguage = language;
            }

            if (GUILayout.Button("Delete", GUILayout.MaxWidth(65)))
            {
                //TODO: mostrar popup para confirmar eliminacion
                if (Enum.TryParse(language, out Data.LANGUAGES languageToRemove)) LocalizationManager.Instance.RemoveLanguageFromCSV(languageToRemove);
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region BASE

        private void ShowHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(10);
            GUILayout.Label(name, HeaderStyle(), options);
            GUILayout.Space(15);
        }

        private void ShowLabelEnumPopupSelection<T>(string label, ref T groupValue) where T : Enum
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            groupValue = (T)EditorGUILayout.EnumPopup(groupValue);
            GUILayout.EndVertical();
        }

        private void ShowLabelTextField(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            keyValue = EditorGUILayout.TextField(keyValue);
            GUILayout.EndVertical();
        }

        private void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, FeedbackLabelStyle());
        }

        private async void ControlTextAreaFeedbackDuration(float durationInSeconds, Data.GUI_SECTIONS feedback)
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

        private static void ShowVerticalLine(float width)
        {
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(width));
        }

        private static void ShowHorizontalLine(float height)
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
        }

        private void VerticalReDimensionalDivisionLine(float width)
        {
            var dividerRect = new Rect(width, 0f, dividerWidth, position.height);
            EditorGUIUtility.AddCursorRect(dividerRect, MouseCursor.ResizeHorizontal);
            EditorGUI.DrawRect(dividerRect, Color.black);
            RedimensionEvent(dividerRect);
        }

        private void RedimensionEvent(Rect dividerRect)
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

        private float GetWidthSize(float percent)
        {
            return percent * _windowSize.x;
        }

        private GUILayoutOption MinWidthOption(float width)
        {
            return GUILayout.MinWidth(width);
        }

        private GUILayoutOption MinHeightOption(float height)
        {
            return GUILayout.MinHeight(height);
        }

        private GUILayoutOption MaxHeightOption(float height)
        {
            return GUILayout.MaxHeight(height);
        }

        #region Styles

        private GUIStyle FixedWidthStyle(float width)
        {
            var style = new GUIStyle
            {
                fixedWidth = width
            };

            return style;
        }

        private GUIStyle SubSectionHeaderStyle()
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

        private GUIStyle FeedbackLabelStyle()
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

        private GUIStyle ButtonStyle()
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

        private GUIStyle HeaderStyle()
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

        private GUIStyle KeyLabelStyle()
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

        private GUIStyle TextFieldValueStyle()
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

        private GUIStyle GroupSelectionStyle()
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
using System;
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

        protected const string NONE = "None";

        #endregion

        #region EDITOR VARIABLES

        private int _currentWindowToolbarIndex;

        private Vector2 _scrollRight;

        private Enums.GUI_WINDOW _currentWindow;
        private DictionaryEditor _dictionaryEditor;
        private LanguagesEditor _languagesEditor;
        private CategoriesEditor _categoriesEditor;
        private ConfigurationEditor _configurationEditor;

        protected readonly string[] _csvSeparators = { ",", ";", ".", ":", "|", "=" };

        #endregion

        #region DIMENSION VARIABLES

        protected static readonly Vector2 _windowSize = new(1400, 750);

        #endregion

        [MenuItem("Tool/LocalizationEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationEditor));
            window.minSize = _windowSize;
            window.titleContent = new GUIContent("Localization Tool");
        }

        protected void OnGUI()
        {
            LoadData();
            if (!LocalizationManager.Instance.IsInitalized) return;
            WindowToolbar();
            switch (_currentWindow)
            {
                case Enums.GUI_WINDOW.Dictionary:
                    ShowDictionaryLayout();
                    break;
                case Enums.GUI_WINDOW.Languages:
                    ShowLanguagesLayout();
                    break;
                case Enums.GUI_WINDOW.Categories:
                    ShowCategoriesLayout();
                    break;
                case Enums.GUI_WINDOW.Configuration:
                    ShowConfigurationLayout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            switch (_currentWindow)
            {
                case Enums.GUI_WINDOW.Dictionary:
                    //TODO
                    break;
                case Enums.GUI_WINDOW.Languages:
                    // TODO
                    break;
                case Enums.GUI_WINDOW.Categories:
                    // TODO
                    break;
                case Enums.GUI_WINDOW.Configuration:
                    // TODO
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
                _currentWindow = Enums.GUI_WINDOW.Dictionary;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Languages"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Languages;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Categories"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Categories;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Configuration"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Configuration;
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region COMMONS

        #region GUI ELEMENTS

        protected void ShowHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(5);
            GUILayout.Label(name, HeaderStyle(), options);
            GUILayout.Space(10);
        }

        protected void ShowExportSubHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, ExportSubHeaderStyle(), options);
            GUILayout.Space(10);
        }

        protected void ShowSubHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, SubHeaderStyle(), options);
            GUILayout.Space(10);
        }

        protected void ShowSectionHeader(string name, params GUILayoutOption[] options)
        {
            GUILayout.Space(8);
            GUILayout.Label(name, SectionHeaderStyle(), options);
            GUILayout.Space(10);
        }

        protected void ShowLabelPopupSelection(string label, ref string categoryValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            GUILayout.Space(5);

            if (LocalizationManager.Categories != null)
            {
                if (LocalizationManager.Categories.IndexOf(categoryValue) != -1)
                {
                    var index = EditorGUILayout.Popup(LocalizationManager.Categories.IndexOf(categoryValue), LocalizationManager.Categories.ToArray(), AddCategoryStyle());
                    categoryValue = LocalizationManager.Categories[index];
                }
                else
                {
                    EditorGUILayout.Popup(0, LocalizationManager.Categories.ToArray(), AddCategoryStyle());
                    if (LocalizationManager.Categories.Count > 0) categoryValue = LocalizationManager.Categories[0];
                }
            }

            GUILayout.EndVertical();
        }

        protected void ShowLabelTextFieldVertical(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUI.SetNextControlName(label);
            GUILayout.Label(label, SubSectionHeaderStyle());
            GUILayout.Space(5);
            keyValue = EditorGUILayout.TextField(keyValue, AddTextFieldStyle());
            GUILayout.EndVertical();
        }

        protected void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, FeedbackLabelStyle());
        }

        internal virtual void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            Repaint();
        }

        public static void ShowVerticalLine(float width)
        {
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(width));
        }

        public static void ShowHorizontalLine(float height)
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
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

        #region Styles

        protected GUIStyle CenterButtonWithIconStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 24,
                fixedWidth = 28
            };

            return style;
        }

        protected GUIStyle BiggerButtonWithIconStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 30,
                fixedWidth = 30
            };

            return style;
        }

        protected GUIStyle AddTextFieldStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fixedHeight = 25,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle FoldoutHeaderStyle()
        {
            var style = new GUIStyle(EditorStyles.foldoutHeader)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 24,
                fontSize = 14,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SeparatorCSVStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.UpperCenter,
                fixedHeight = 28,
                fixedWidth = 55,
                fontSize = 18,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SeparatorCSVLanguageWindowStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 22,
                fixedWidth = 45,
                fontSize = 15,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle CategoryPopupStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 25,
                fixedWidth = 250,
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle AddCategoryStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24,
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SearchTypeStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24,
                fixedWidth = 150,
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SubSectionHeaderStyle(float fixedWidth)
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                fixedWidth = fixedWidth,
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

        protected GUIStyle ExportSubHeaderStyle()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 14,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SubHeaderStyle(TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            var style = new GUIStyle
            {
                alignment = textAnchor,
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SectionHeaderStyle()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 15,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }


        protected GUIStyle SubSectionHeaderStyle()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle SelectableLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fixedHeight = 24,
                fixedWidth = 300,
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
                fixedHeight = 24,
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
                richText = true,
                wordWrap = true,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };

            return style;
        }

        protected GUIStyle IntFieldValueStyle()
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
#endif
}
using System;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editor
{
    public class LocalizationEditor : EditorWindow
    {
        #region EDITOR VARIABLES
        
        private int _currentWindowToolbarIndex;
        
        private Vector2 _scrollRight;
        
        private Enums.GUI_WINDOW _currentWindow;
        private DictionaryEditor _dictionaryEditor;
        private LanguagesEditor _languagesEditor;
        
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
        
        private void OnGUI()
        {
            LoadData();
            WindowToolbar();
            switch (_currentWindow)
            {
                case Enums.GUI_WINDOW.Dictionary: ShowDictionaryLayout();
                    break;
                case Enums.GUI_WINDOW.Languages: ShowLanguagesLayout();
                    break;
                case Enums.GUI_WINDOW.Categories: ShowCategoriesLayout();
                    break;
                case Enums.GUI_WINDOW.Configuration: ShowConfigurationLayout();
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
            _dictionaryEditor ??= (DictionaryEditor) CreateInstance(typeof(DictionaryEditor));
            _dictionaryEditor.ShowLayout();
        }
        
        private void ShowLanguagesLayout()
        {
            _languagesEditor ??= (LanguagesEditor) CreateInstance((typeof(LanguagesEditor)));
            _languagesEditor.ShowLayout();
        }
        
        private void ShowCategoriesLayout()
        {
            
        }
        
        private void ShowConfigurationLayout()
        {
            
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
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Languages"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Languages;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Categories"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Categories;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Configuration"), EditorStyles.toolbarButton))
            {
                _currentWindow = Enums.GUI_WINDOW.Configuration;
            }
            GUILayout.Space(10);

            GUILayout.EndHorizontal();
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
            GUILayout.Space(5);
            //TODO: EditorGUILayout.Popup();
            groupValue = (T)EditorGUILayout.EnumPopup(groupValue, AddGroupStyle());
            GUILayout.EndVertical();
        }

        protected void ShowLabelTextFieldVertical(string label, ref string keyValue)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label, SubSectionHeaderStyle());
            GUILayout.Space(5);
            keyValue = EditorGUILayout.TextField(keyValue, AddTextFieldStyle());
            GUILayout.EndVertical();
        }

        protected void ShowTextAreaFeedback(string label)
        {
            EditorGUILayout.LabelField(label, FeedbackLabelStyle());
        }

        internal virtual async void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            ;
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

        protected GUIStyle AddGroupStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 25,
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
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
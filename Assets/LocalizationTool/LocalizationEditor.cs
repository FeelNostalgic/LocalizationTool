using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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

        #endregion

        #region DIMENSION VARIABLES

        private static readonly Vector2 _windowSize = new(1400, 750);
        private float _leftSectionWidthPercent = 0.225f;
        private float _rigthSectionWidthPercent = 0.225f;

        private float _dividerPosition = 305f;
        private const float dividerWidth = 5f;

        private bool isResizingDivider = false;

        #endregion

        [MenuItem("Tool/LocalizationEditor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(LocalizationEditor));
            window.minSize = _windowSize;
            window.titleContent = new GUIContent("Localization Tool");
            LocalizationManager.Instance.Init();
            if (LocalizationManager.ActiveLanguages.Count > 0) _currentLanguage = LocalizationManager.ActiveLanguages[0].ToString();
        }

        private void OnGUI()
        {
            ShowLayout();
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
            ShowRemoveSection();
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
                //TODO: 
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
            ShowHeader(_currentLanguage);

            ShowHorizontalLine(5);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("KEY", SubSectionHeaderStyle());
            GUILayout.Space(10);
            GUILayout.Label("GROUP", SubSectionHeaderStyle(), GUILayout.MaxWidth(250));
            GUILayout.Space(10);
            GUILayout.Label("VALUE", SubSectionHeaderStyle());
            GUILayout.Space(10);

            GUILayout.EndHorizontal();

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void GenerateCenterScrollViewContent()
        {
            //TODO
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            for (int i = 0; i < 25; i++)
            {
                UnitCenterScrollViewContent();
            }
            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("mainMenu_OK", KeyLabelStyle(), MinHeightOption(24));
            GUILayout.Space(10);
            _searchGroupValue = (Data.GROUPS)EditorGUILayout.EnumPopup(_searchGroupValue, GroupSelectionStyle(), MinHeightOption(24), GUILayout.MaxWidth(250));
            GUILayout.Space(10);
            EditorGUILayout.TextField("Ok", TextFieldValueStyle(), MinHeightOption(24));
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

        private async void ShowLanguageSection()
        {
            try
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
                    //TODO: 
                    _addLanguageFeedbackLabelText = await LocalizationManager.Instance.AddNewLanguageToCSV(_addLanguageValue);

                    var durationInSeconds = 1f;
                    _addLanguageFeedbackLabelText = await ControlTextAreaFeedbackDuration((int)(durationInSeconds * 1000));
                }

                GUILayout.Space(10);

                ShowTextAreaFeedback(_addLanguageFeedbackLabelText);

                GUILayout.EndVertical();

                ShowHorizontalLine(5);

                GenerateRightScrollViewContent();
                
                GUILayout.EndVertical();
            }
            catch (Exception e)
            {
            }
        }

        private void GenerateRightScrollViewContent()
        {
            _scrollRight = EditorGUILayout.BeginScrollView(_scrollRight);
            foreach (var t in LocalizationManager.ActiveLanguages)
            {
                UnitRightScrollViewContent(t.ToString());
            }
            EditorGUILayout.EndScrollView();
        }

        private void UnitRightScrollViewContent(string language)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(language))
            {
                //TODO
                _currentLanguage = language;
            }

            if (GUILayout.Button("Delete", GUILayout.MaxWidth(65)))
            {
                //TODO: mostrar popup para confirmar eliminacion
                if (Enum.TryParse(language, out Data.LANGUAGES languageToRemove)) LocalizationManager.Instance.RemoveLanguageFromCVS(languageToRemove);
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region BASE

        private void ShowHeader(string name)
        {
            GUILayout.Space(10);
            GUILayout.Label(name, HeaderStyle());
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

        private async Task<string> ControlTextAreaFeedbackDuration(int durationInMilliseconds)
        {
            await Task.Delay(durationInMilliseconds);
            return "";
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
using System.Threading.Tasks;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    public class LanguagesEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        public string AddLanguageFeedbackLabelText
        {
            get => _addLanguageFeedbackLabelText;
            set => _addLanguageFeedbackLabelText = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _addLanguageValue;
        private string _addLanguageFeedbackLabelText = "";

        private Vector2 _scrollCenter;
        private const int MAX_CHARACTERS = 40;

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.25f;

        private float _dividerPosition = 305f;
        private const float dividerWidth = 5f;

        private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public void ShowLayout()
        {
            GUILayout.BeginVertical(MinHeightOption(_windowSize.y));

            GUILayout.Space(5);
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
            GUILayout.Space(5);
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));
            GUILayout.FlexibleSpace();
            ShowAddSection();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ShowAddSection()
        {
            ShowHorizontalLine(5);
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            
            GUILayout.Label("LANGUAGE", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(3);

            GUI.SetNextControlName("LANGUAGE");
            _addLanguageValue = EditorGUILayout.TextField(_addLanguageValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));

            GUILayout.Space(5);

            if (GUILayout.Button(GetGUIContent(AddIcon, "Add new language"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LocalizationManager.Instance.AddNewLanguage(_addLanguageValue, this);
                ControlTextAreaFeedbackDuration(1f);
            }

            if (GUI.GetNameOfFocusedControl() == "LANGUAGE")
            {
                if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                {
                    LocalizationManager.Instance.AddNewLanguage(_addLanguageValue, this);
                    ControlTextAreaFeedbackDuration(1.5f);
                }
            }

            GUILayout.Space(3);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            ShowTextAreaFeedback(_addLanguageFeedbackLabelText);
            
            GUILayout.EndVertical();
            
            ShowHorizontalLine(5);
        }

        #endregion

        #region CENTER SECTION

        private void ShowCenterSection()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);

            TitleCenterSection();

            ShowHorizontalLine(5);

            GUILayout.Space(10);

            GenerateCenterScrollViewContent();

            GUILayout.EndVertical();
        }

        private void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(GetGUIContent(RefreshIcon, "Reload data"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.Instance.RefreshLanguagesData();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("LANGUAGES",CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                foreach (var (index, language) in LocalizationManager.OrderedLanguages)
                {
                    UnitCenterScrollViewContent(index, language);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(int index, string language)
        {
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginHorizontal("box", _height);
            GUI.backgroundColor = Color.clear;
            GUILayout.Space(10);

            Column1(index);

            Column2(index, language);

            Column3(index, language);

            Column4(language);
            
            Column5(language);
            
            GUILayout.EndHorizontal();
        }

        private void Column1(int index)
        {
            //Number
            GUILayout.BeginVertical("box", GUILayout.Width(35), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            var tempIndex = EditorGUILayout.IntField(index, CustomStyles.GetStyle(Enums.CustomStyleName.OrderIntField), GUILayout.Height(30));      
            if(EditorGUI.EndChangeCheck()) UpdateIndex(index, tempIndex);
            GUILayout.EndVertical();
        }

        private void Column2(int index, string language)
        {
            //Button To move UP
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(GetGUIContent(UpIcon, $"Move '{language}' Up"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                UpdateIndex(index, index - 2);
            }
            GUILayout.EndVertical();

        }

        private void Column3(int index, string language)
        {
            //Button To move DOWN
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(DownIcon, $"Move '{language}' Down"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                UpdateIndex(index, index + 2);
            }
            GUILayout.EndVertical();
        }

        private void Column4(string language)
        {
            // Favourite Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(26), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            if (LocalizationManager.IsFavouriteLanguage(language))
            {
                if (GUILayout.Button(GetGUIContent(LocalizationManager.Instance.YellowIcon, $"'{language}' is favourite language"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    // Nothing
                }
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(FavouriteIcon, $"Make '{language}' favourite"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    LocalizationManager.Instance.ChangeFavoriteLanguage(language);
                    LocalizationManager.Log($"Language '{language}' is now favorite");
                }
            }
            GUILayout.EndVertical();
        }

        private void Column5(string language)
        {
            // FIELDS
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", _height, GUILayout.ExpandWidth(true));
            GUI.backgroundColor = Colors.DEFAULT;

            // Text
            var tempValue = EditorGUILayout.TextField(language, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.Height(40));
            if (tempValue.Length > MAX_CHARACTERS) tempValue = tempValue[..MAX_CHARACTERS];
            UpdateLanguage(language, tempValue);
            
            // BUTTONS
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", GUILayout.Width(23*2+2*2));
            GUI.backgroundColor = Colors.DEFAULT;
            
            // Import Button
            if (GUILayout.Button(GetGUIContent(GearIcon, "Open Manage Menu"),  CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LanguageManagerEditor.ShowWindow(language);
            }
            
            //GUILayout.Space(2);

            // Export Button
            // if (GUILayout.Button(GetGUIContent(ExportIcon, "Open Export Menu"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            // {
            //     LanguageExportEditor.ShowWindow(language);
            // }
            
            GUILayout.Space(2);

            // Delete button
            if (LocalizationManager.IsFavouriteLanguage(language))
            {
                if (GUILayout.Button(GetGUIContent(LockIcon, "Favourite language cannot be removed"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                }
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(DeleteIcon, "Remove language"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (LocalizationManager.Configuration.LanguageDeleteConfirmation)
                    {
                        if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {language}?", "Delete", "Cancel"))
                        {
                            LocalizationManager.Instance.RemoveLanguage(language);
                            LocalizationManager.Log($"Language '{language}' removed");
                        }
                    }
                    else
                    {
                        LocalizationManager.Instance.RemoveLanguage(language);
                        LocalizationManager.Log($"Language '{language}' removed");
                    }
                }
            }

            GUILayout.EndHorizontal();
            
            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

        private static void UpdateLanguage(string oldLanguage, string newValue)
        {
            LocalizationManager.Instance.ChangeLanguageValue(oldLanguage, newValue);
        }
        
        private static void UpdateIndex(int oldIndex, int newIndex)
        {
            LocalizationManager.Instance.ChangeLanguageIndex(oldIndex, newIndex);
        }

        #endregion

        internal override async void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            var millisecondsDelay = (int)(durationInSeconds * 1000);
            await Task.Delay(millisecondsDelay);
            _addLanguageFeedbackLabelText = "";
        }

        #endregion
    }
#endif
}
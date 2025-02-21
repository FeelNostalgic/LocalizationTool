#if UNITY_EDITOR

using System.Collections.Generic;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.General;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
{
    public class LanguagesEditor : EditorWindowAbstract
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _addLanguageValue;

        private Vector2 _scrollCenter;

        private readonly Dictionary<string, string> _feedbackList = new();

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.25f;

        // FUTURE
        // private float _dividerPosition = 305f;
        // private const float dividerWidth = 5f;
        // private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public override void ShowLayout()
        {
            ControlFocus(LANGUAGE_LABEL_UPPER);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

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

        protected override void ControlFocus(string focus)
        {
            if (!LocalizationManager.Configuration.languageClearAdd) return;
            base.ControlFocus(focus);
        }

        #endregion

        #region PRRIVATE METHODS

        #region LEFT SECTION

        private void ShowLeftSection()
        {
            GUILayout.Space(5);
            GUILayout.BeginVertical(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT, WindowSize.x)));
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

            GUILayout.Label(LANGUAGE_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(3);

            GUI.SetNextControlName(LANGUAGE_LABEL_UPPER);
            _addLanguageValue = EditorGUILayout.TextField(_addLanguageValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));

            GUILayout.Space(5);

            if (GUILayout.Button(GetGUIContent(AddIcon, ADD_LANGUAGE_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LocalizationManager.AddNewLanguage(_addLanguageValue, this);
            }

            if (GUI.GetNameOfFocusedControl() == LANGUAGE_LABEL_UPPER)
            {
                if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                {
                    if (!AddActionRunning) LocalizationManager.AddNewLanguage(_addLanguageValue, this);
                }
            }

            GUILayout.Space(3);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            ShowTextAreaFeedback(FeedbackLabel);

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

        private static void TitleCenterSection()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(GetGUIContent(RefreshIcon, RELOAD_BUTTON_TOOLTIP), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.RefreshLanguagesData();
                LocalizationManager.Log(LANGUAGES_LABEL, CustomDebugPlugin.Colors.Purple, LOADED_LOG);
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(LANGUAGES_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            
            try
            {
                foreach (var language in CacheDataSO.LanguageCache)
                {
                    UnitCenterScrollViewContent(language.displayOrder, language.languageName);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(int index, string languageName)
        {
            var height = _feedbackList.ContainsKey(languageName) ? Height : GUILayout.Height(40 + 20);
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginVertical("box", height);

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal(Height);

            GUILayout.Space(10);

            Column1(index, languageName);

            Column2(index, languageName);

            Column3(index, languageName);

            Column4(languageName);

            Column5(languageName);

            GUILayout.EndHorizontal();

            Feedback(languageName);

            GUILayout.EndVertical();
        }

        private void Column1(int index, string languageName)
        {
            //Number
            GUILayout.BeginVertical("box", GUILayout.Width(35), Height);
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            var tempIndex = EditorGUILayout.IntField(index, CustomStyles.GetStyle(Enums.CustomStyleName.OrderIntField), GUILayout.Height(30));
            if (EditorGUI.EndChangeCheck()) UpdateIndex(languageName, index, tempIndex);
            GUILayout.EndVertical();
        }

        private void Column2(int index, string languageName)
        {
            //Button To move UP
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(GetGUIContent(UpIcon, string.Format(MOVE_UP_BUTTON_TOOLTIP, languageName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultLanguage(languageName)) UpdateIndex(languageName, index, index - 1);
            }

            GUILayout.EndVertical();
        }

        private void Column3(int index, string languageName)
        {
            //Button To move DOWN
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(DownIcon, string.Format(MOVE_DOWN_BUTTON_TOOLTIP, languageName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultLanguage(languageName) && CacheDataSO.LanguageCache.Count >= index + 1) UpdateIndex(languageName, index, index + 1);
            }

            GUILayout.EndVertical();
        }

        private void Column4(string languageName)
        {
            // Favourite Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(26), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (LocalizationManager.IsDefaultLanguage(languageName))
            {
                if (GUILayout.Button(GetGUIContent(YellowStarIcon, string.Format(LANGUAGE_IS_FAVOURITE_BUTTON_TOOLTIP, languageName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    // Nothing
                }
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(StarIcon, string.Format(MAKE_LANGUAGE_FAVOURITE_BUTTON_TOOLTIP, languageName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    LocalizationManager.ChangeDefaultLanguage(languageName);
                    LocalizationManager.Log(LANGUAGES_LABEL, CustomDebugPlugin.Colors.Purple, string.Format(LANGUAGE_FAVOURITE_LOG, languageName));
                }
            }

            GUILayout.EndVertical();
        }

        private void Column5(string languageName)
        {
            // FIELDS
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", Height, GUILayout.ExpandWidth(true));
            GUI.backgroundColor = Colors.DEFAULT;

            // Text
            EditorGUI.BeginChangeCheck();
            var tempValue = EditorGUILayout.TextField(languageName, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.Height(40));

            if (EditorGUI.EndChangeCheck()) UpdateLanguage(languageName, tempValue);

            // BUTTONS
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", GUILayout.Width(23 * 2 + 2 * 2));
            GUI.backgroundColor = Colors.DEFAULT;

            // Import Button
            if (GUILayout.Button(GetGUIContent(GearIcon, OPEN_MANAGE_MENU_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LanguageManagerEditor.ShowWindow(languageName);
            }

            GUILayout.Space(2);

            // Delete button
            if (LocalizationManager.IsDefaultLanguage(languageName))
            {
                if (GUILayout.Button(GetGUIContent(LockIcon, DELETE_LANGUAGE_FAVOURITE_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                }
            }
            else
            {
                var content = LocalizationManager.Configuration.deleteOrEmptyLanguageIndex == 0
                    ? GetGUIContent(DeleteIcon, string.Format(DELETE_LANGUAGE_TOOLTIP, languageName)) //Delete
                    : GetGUIContent(EmptyIcon, string.Format(EMPTY_LANGUAGE_TOOLTIP, languageName)); //Empty

                if (GUILayout.Button(content, CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (LocalizationManager.Configuration.languageDeleteConfirmation)
                    {
                        if (LocalizationManager.Configuration.deleteOrEmptyLanguageIndex == 0)
                        {
                            if (EditorUtility.DisplayDialog(DELETE_DIALOG_TITLE, string.Format(DELETE_DIALOG_MESSAGE, languageName), DIALOG_OPTION_DELETE, DIALOG_OPTION_CANCEL))
                                DeleteLanguage(languageName);
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog(EMPTY_DIALOG_TITLE, string.Format(EMPTY_DIALOG_MESSAGE, languageName), DIALOG_OPTION_EMPTY, DIALOG_OPTION_CANCEL))
                                EmptyLanguage(languageName);
                        }
                    }
                    else
                    {
                        if (LocalizationManager.Configuration.deleteOrEmptyLanguageIndex == 0)
                            DeleteLanguage(languageName);
                        else
                            EmptyLanguage(languageName);
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private static void DeleteLanguage(string languageName)
        {
            LocalizationManager.RemoveLanguage(languageName);
            LocalizationManager.Log(LANGUAGES_LABEL, CustomDebugPlugin.Colors.Purple, string.Format(DELETED_LANGUAGE_LOG, languageName));
        }

        private static void EmptyLanguage(string languageName)
        {
            LocalizationManager.EmptyLanguage(languageName);
            LocalizationManager.Log(LANGUAGES_LABEL, CustomDebugPlugin.Colors.Purple, string.Format(EMPTIED_LANGUAGE_LOG, languageName));
        }
        
        private void Feedback(string languageName)
        {
            //Feedback
            if (_feedbackList.ContainsKey(languageName) && _feedbackList[languageName].IsNotEmpty())
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.FlexibleSpace();
                GUILayout.Label(WarningIcon, GUILayout.Width(20));
                GUILayout.Label(_feedbackList[languageName], CustomStyles.GetStyle(Enums.CustomStyleName.RichTextEditorFeedbackLabel));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        #endregion

        #region UPDATE METHODS

        private void UpdateLanguage(string oldLanguage, string newLanguage)
        {
            if (oldLanguage.Equals(newLanguage)) return;

            if (!_feedbackList.ContainsKey(oldLanguage)) _feedbackList.Add(oldLanguage, "");

            if (newLanguage.IsEmpty())
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldLanguage] = s; }, EMPTY_LANGUAGE_FEEDBACK_LABEL, delegate { _feedbackList.Remove(oldLanguage); });
                return;
            }

            if (newLanguage.Length > MAX_LANGUAGE_CHARACTERS)
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldLanguage] = s; }, string.Format(CHARACTERS_NUMBER_LANGUAGE_FEEDBACK_LABEL, MAX_LANGUAGE_CHARACTERS),
                    delegate { _feedbackList.Remove(oldLanguage); });
                return;
            }

            if (CacheDataSO.Languages.Contains(newLanguage))
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldLanguage] = s; }, string.Format(LANGUAGE_EXIST_FEEDBACK_LABEL, newLanguage), delegate { _feedbackList.Remove(oldLanguage); });
                return;
            }

            LocalizationManager.ChangeLanguageValue(oldLanguage, newLanguage);

            _feedbackList.Remove(oldLanguage);
        }

        private static void UpdateIndex(string languageName, int oldIndex, int newIndex)
        {
            LocalizationManager.ChangeLanguageIndex(languageName, oldIndex, newIndex);
        }

        public override void ClearAddTextField()
        {
            if (!LocalizationManager.Configuration.languageClearAdd) return;
            _addLanguageValue = "";
            RepaintGUI();
        }

        #endregion

        #endregion
    }
}
#endif
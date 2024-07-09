using System.Threading.Tasks;
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
            GUILayout.Space(10);

            GUILayout.Label("LANGUAGE", SubSectionHeaderStyle());
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            
            GUI.SetNextControlName("LANGUAGE");
            _addLanguageValue = EditorGUILayout.TextField(_addLanguageValue, MinHeightOption(25));

            GUILayout.Space(5);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ol_plus", "Add new language"), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25)))
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

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            ShowTextAreaFeedback(_addLanguageFeedbackLabelText);

            GUILayout.Space(10);
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
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh", "Refresh data loading"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
            {
                LocalizationManager.Instance.RefreshLanguagesData();
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label("LANGUAGES", HeaderStyle());
            
            GUILayout.FlexibleSpace();
            
            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                foreach (var language in LocalizationManager.ActiveLanguages)
                {
                    UnitCenterScrollViewContent(language);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(string language)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            
            if (LocalizationManager.Instance.FavouriteLanguage.Equals(language))
            {
                if (GUILayout.Button(LocalizationManager.Instance.YellowIcon, CenterButtonWithIconStyle()))
                {
                    // Nothing
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Favorite", "Make favourite"), CenterButtonWithIconStyle()))
                {
                    LocalizationManager.Instance.ChangeFavoriteLanguage(language);
                    LocalizationManager.Log($"Language '{language}' is now favorite");
                }
            }
            
            GUILayout.Space(10);

            var tempValue = EditorGUILayout.TextField(language, TextFieldValueStyle(), MinHeightOption(24));
            UpdateLanguage(language, tempValue);

            GUILayout.Space(10);
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), CenterButtonWithIconStyle()))
            {
                LanguageImportEditor.ShowWindow(language);
            }
           
            GUILayout.Space(10);
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), CenterButtonWithIconStyle()))
            {
                LanguageExportEditor.ShowWindow(language);
            }
            
            GUILayout.Space(10);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete"), CenterButtonWithIconStyle()))
            {
                if (LocalizationManager.ActiveLanguages.Count == 1)
                {
                    if (EditorUtility.DisplayDialog("Warning", $"Al least you need ONE active language", "Ok"))
                    {
                        //Nothing
                    }
                }
                else
                {
                    if (LocalizationManager.Instance.FavouriteLanguage.Equals(language))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Favourite language can not be removed", "Ok"))
                        {
                        }
                    }
                    else
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
            }

            GUILayout.Space(10);

            GUILayout.EndHorizontal();
        }

        #endregion

        #region UPDATE METHODS

        private static void UpdateLanguage(string oldLanguage, string newValue)
        {
            LocalizationManager.Instance.ChangeLanguageValue(oldLanguage, newValue);
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
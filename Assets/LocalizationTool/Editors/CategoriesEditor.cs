using System;
using System.Threading.Tasks;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    public class CategoriesEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        public string AddCategoryFeedbackLabelText
        {
            get => _addCategoryFeedbackLabelText;
            set => _addCategoryFeedbackLabelText = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _addCategoryValue;
        private string _addCategoryFeedbackLabelText = "";

        private Vector2 _scrollCenter;

        private const int MAX_CHARACTERS = 100;

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

            GUILayout.Label("CATEGORY", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter14Label));
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            GUI.SetNextControlName("CATEGORY");
            _addCategoryValue = EditorGUILayout.TextField(_addCategoryValue, MinHeightOption(25));

            GUILayout.Space(5);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ol_plus", "Add new language"), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25)))
            {
                LocalizationManager.Instance.AddNewCategory(_addCategoryValue, this);
                ControlTextAreaFeedbackDuration(1f);
            }

            if (GUI.GetNameOfFocusedControl() == "CATEGORY")
            {
                if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                {
                    LocalizationManager.Instance.AddNewCategory(_addCategoryValue, this);
                    ControlTextAreaFeedbackDuration(1f);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            ShowTextAreaFeedback(_addCategoryFeedbackLabelText);

            GUILayout.Space(10);
            ShowHorizontalLine(5);
        }

        #endregion

        #region CENTER SECTION

        private void ShowCenterSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

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
                LocalizationManager.Instance.RefreshCategoriesData();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("CATEGORIES", CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            try
            {
                foreach (var (index, category) in LocalizationManager.OrderedCategories)
                {
                    UnitCenterScrollViewContent(index, category);
                }
            }
            catch
            {
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnitCenterScrollViewContent(int index, string category)
        {
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginHorizontal("box", _height);
            GUI.backgroundColor = Color.clear;
            GUILayout.Space(10);

            Column1(index, category);
            
            Column2(index, category);
            
            Column3(index, category);

            Column4(category);
            
            Column5(category);
            
            GUILayout.EndHorizontal();
        }

        private void Column1(int index, string category)
        {
            //Number
            GUILayout.BeginVertical("box", GUILayout.Width(35), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            var tempIndex = EditorGUILayout.IntField(index, CustomStyles.GetStyle(Enums.CustomStyleName.OrderIntField), GUILayout.Height(30));
            if (!LocalizationManager.IsDefaultCategory(category)) UpdateIndex(index, tempIndex);
            GUILayout.EndVertical();
        }

        private void Column2(int index, string category)
        {
            //Button To move UP
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrollup", "Locked"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(category)) UpdateIndex(index, index - 2);
            }
            GUILayout.EndVertical();
        }

        private void Column3(int index, string category)
        {
            //Button To move DOWN
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrolldown", "Locked"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(category)) UpdateIndex(index, index + 2);
            }
            GUILayout.EndVertical();
        }

        private void Column4(string category)
        {
            // Favourite Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(26), _height);
            GUI.backgroundColor = Colors.DEFAULT;
            
            GUILayout.FlexibleSpace();
            if (LocalizationManager.IsDefaultCategory(category))
            {
                if (GUILayout.Button(LocalizationManager.Instance.YellowIcon, CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    // Nothing
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Favorite", "Make favourite"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    LocalizationManager.Instance.ChangeDefaultCategory(category);
                    LocalizationManager.Log($"Category '{category}' is now default");
                }
            }
            GUILayout.EndVertical();
        }

        private void Column5(string category)
        {
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", _height, GUILayout.ExpandWidth(true));
            GUI.backgroundColor = Colors.DEFAULT;
            
            // FIELDS

            // Text
            var tempValue = EditorGUILayout.TextField(category, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.Height(40));
            if (tempValue.Length > MAX_CHARACTERS) tempValue = tempValue[..MAX_CHARACTERS];
            UpdateCategory(category, tempValue);
            
            // Delete Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23));
            GUI.backgroundColor = Colors.DEFAULT;
            GUILayout.Space(2);
            if (LocalizationManager.IsDefaultCategory(category))
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_AssemblyLock", "Locked"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (EditorUtility.DisplayDialog("Warning", $"You can not remove Default category", "Ok"))
                    {
                    }
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (LocalizationManager.Configuration.CategoryDeleteConfirmation)
                    {
                        if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {category}?", "Delete", "Cancel"))
                        {
                            LocalizationManager.Instance.RemoveCategory(category);
                            LocalizationManager.Log($"Category '{category}' removed");
                        }
                    }
                    else
                    {
                        LocalizationManager.Instance.RemoveCategory(category);
                        LocalizationManager.Log($"Category '{category}' removed");
                    }
                }
            }
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }
        
        #endregion

        #region UPDATE METHODS

        private static void UpdateCategory(string oldLanguage, string newValue)
        {
            LocalizationManager.Instance.ChangeCategoryName(oldLanguage, newValue);
        }

        private static void UpdateIndex(int oldIndex, int newIndex)
        {
            LocalizationManager.Instance.ChangeCategoryIndex(oldIndex, newIndex);
        }

        #endregion

        internal override async void ControlTextAreaFeedbackDuration(float durationInSeconds)
        {
            var millisecondsDelay = (int)(durationInSeconds * 1000);
            await Task.Delay(millisecondsDelay);
            _addCategoryFeedbackLabelText = "";
        }

        #endregion
    }
#endif
}
#if UNITY_EDITOR

using System;
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
    public class CategoriesEditor : EditorWindowAbstract
    {
        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _addCategoryValue;

        private Vector2 _scrollCenter;
        private float _scrollCenterPreviousY;

        private readonly Dictionary<string, string> _feedbackList = new();

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.25f;

        // FUTURE: divider
        // private float _dividerPosition = 305f;
        // private const float dividerWidth = 5f;
        // private bool isResizingDivider = false;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public override void ShowLayout()
        {
            ControlFocus(CATEGORY_LABEL_UPPER);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            GUILayout.Space(5);
            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal();
            ShowLeftSection();

            ShowVerticalLine(5);
            // FUTURE: VerticalReDimensionalDivisionLine(_leftSectionWidth);

            ShowCenterSection();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        protected override void ControlFocus(string focus)
        {
            if (!LocalizationManager.Configuration.categoryClearAdd) return;
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

            GUILayout.Label(CATEGORY_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(3);

            GUI.SetNextControlName(CATEGORY_LABEL_UPPER);
            _addCategoryValue = EditorGUILayout.TextField(_addCategoryValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));

            GUILayout.Space(5);

            if (GUILayout.Button(GetGUIContent(AddIcon, ADD_CATEGORY_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LocalizationManager.AddNewCategory(_addCategoryValue, this);
            }

            if (GUI.GetNameOfFocusedControl() == CATEGORY_LABEL_UPPER)
            {
                if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                {
                    if (!AddActionRunning) LocalizationManager.AddNewCategory(_addCategoryValue, this);
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
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

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
                LocalizationManager.RefreshCategoriesData();
                LocalizationManager.Log(CATEGORIES_LABEL, CustomDebug.Colors.Green, LOADED_LOG);
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(CATEGORIES_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void GenerateCenterScrollViewContent()
        {
            _scrollCenter = EditorGUILayout.BeginScrollView(_scrollCenter);
            if (!Mathf.Approximately(_scrollCenter.y, _scrollCenterPreviousY)) // If there are no equals, scroll is moving => clear focus
            {
                _scrollCenterPreviousY = _scrollCenter.y;
                GUI.FocusControl("");
            }

            const float itemHeight = 40f;
            const int visibleItemsCount = 50;

            try
            {
                GUILayout.BeginVertical();

                var firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(_scrollCenter.y / visibleItemsCount));
                var lastVisibleIndex = Mathf.Min(firstVisibleIndex + visibleItemsCount, CacheDataSO.CategoryCache.Count);

                GUILayout.Space(firstVisibleIndex * itemHeight);

                for (var i = firstVisibleIndex; i < lastVisibleIndex; i++)
                {
                    var category = CacheDataSO.CategoryCache[i];
                    DrawCategoryItem(category.displayOrder, category.categoryName);
                }

                GUILayout.Space(Mathf.Clamp((CacheDataSO.CategoryCache.Count - lastVisibleIndex - 1) * itemHeight, 0f, CacheDataSO.CategoryCache.Count * itemHeight)+40);

                GUILayout.EndVertical();
            }
            catch (Exception e)
            {
                if (e is not ExitGUIException) Debug.LogError(e);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCategoryItem(int index, string categoryName)
        {
            var hasFeedback = _feedbackList.ContainsKey(categoryName);
            var itemHeight = hasFeedback ? GUILayout.Height(60) : Height;

            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginVertical("box", itemHeight);

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal(itemHeight);
            GUILayout.Space(10);

            Column1(index, categoryName);
            Column2(index, categoryName);
            Column3(index, categoryName);
            Column4(categoryName);
            Column5(categoryName);

            GUILayout.EndHorizontal();

            Feedback(categoryName);

            GUILayout.EndVertical();
        }

        private void Column1(int index, string categoryName)
        {
            //Number
            GUILayout.BeginVertical("box", GUILayout.Width(35), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            var tempIndex = EditorGUILayout.DelayedIntField(index, CustomStyles.GetStyle(Enums.CustomStyleName.OrderIntField), GUILayout.Height(30));
            if (!LocalizationManager.IsDefaultCategory(categoryName)) UpdateIndex(categoryName, index, tempIndex);
            GUILayout.EndVertical();
        }

        private void Column2(int index, string categoryName)
        {
            //Button To move UP
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(UpIcon, string.Format(MOVE_UP_BUTTON_TOOLTIP, categoryName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(categoryName)) UpdateIndex(categoryName, index, index - 1);
            }

            GUILayout.EndVertical();
        }

        private void Column3(int index, string categoryName)
        {
            //Button To move DOWN
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(DownIcon, string.Format(MOVE_DOWN_BUTTON_TOOLTIP, categoryName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(categoryName) && CacheDataSO.CategoryCache.Count >= index + 1) UpdateIndex(categoryName, index, index + 1);
            }

            GUILayout.EndVertical();
        }

        private void Column4(string categoryName)
        {
            // Favourite Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(26), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (LocalizationManager.IsDefaultCategory(categoryName))
            {
                if (GUILayout.Button(GetGUIContent(YellowStarIcon, string.Format(CATEGORY_IS_DEFAULT_BUTTON_TOOLTIP, categoryName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                }
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(StarIcon, string.Format(MAKE_CATEGORY_DEFAULT_BUTTON_TOOLTIP, categoryName)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    LocalizationManager.Instance.ChangeDefaultCategory(categoryName);
                    LocalizationManager.Log(CATEGORIES_LABEL, CustomDebug.Colors.Green, string.Format(CATEGORY_DEFAULT_LOG, categoryName));
                }
            }

            GUILayout.EndVertical();
        }

        private void Column5(string category)
        {
            // FIELDS
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box", Height, GUILayout.ExpandWidth(true));
            GUI.backgroundColor = Colors.DEFAULT;

            // Text
            EditorGUI.BeginChangeCheck();
            var tempValue = EditorGUILayout.DelayedTextField(category, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck()) UpdateCategory(category, tempValue);

            // Delete Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23));
            GUI.backgroundColor = Colors.DEFAULT;
            if (LocalizationManager.IsDefaultCategory(category))
            {
                if (GUILayout.Button(GetGUIContent(LockIcon, DELETE_CATEGORY_DEFAULT_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                }
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(DeleteIcon, string.Format(DELETE_CATEGORY_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (LocalizationManager.Configuration.categoryDeleteConfirmation)
                    {
                        if (EditorUtility.DisplayDialog(DELETE_DIALOG_TITLE, string.Format(DELETE_DIALOG_MESSAGE, category), DIALOG_OPTION_DELETE, DIALOG_OPTION_CANCEL))
                            DeleteCategory(category);
                    }
                    else
                        DeleteCategory(category);
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void Feedback(string category)
        {
            //Feedback
            if (_feedbackList.ContainsKey(category) && _feedbackList[category].IsNotEmpty())
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.FlexibleSpace();
                GUILayout.Label(WarningIcon, GUILayout.Width(20));
                GUILayout.Label(_feedbackList[category], CustomStyles.GetStyle(Enums.CustomStyleName.RichTextEditorFeedbackLabel));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        #endregion

        #region UPDATE METHODS

        private static void DeleteCategory(string category)
        {
            LocalizationManager.RemoveCategory(category);
            LocalizationManager.Log(CATEGORIES_LABEL, CustomDebug.Colors.Green, string.Format(DELETED_CATEGORY_LOG, category));
        }

        private void UpdateCategory(string oldCategory, string newCategory)
        {
            if (oldCategory.Equals(newCategory)) return;

            _feedbackList.TryAdd(oldCategory, "");

            if (newCategory.IsEmpty())
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, EMPTY_CATEGORY_FEEDBACK_LABEL, delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            if (newCategory.Length > MAX_CATEGORY_CHARACTERS)
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, string.Format(CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL, MAX_CATEGORY_CHARACTERS),
                    delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            if (LocalizationManager.ExistsCategory(newCategory))
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, string.Format(CATEGORY_EXIST_FEEDBACK_LABEL, newCategory), delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            LocalizationManager.ChangeCategoryName(oldCategory, newCategory);

            _feedbackList.Remove(oldCategory);
        }

        private static void UpdateIndex(string categoryName, int oldIndex, int newIndex)
        {
            LocalizationManager.ChangeCategoryIndex(categoryName, oldIndex, newIndex);
        }

        public override void ClearAddTextField()
        {
            if (!LocalizationManager.Configuration.categoryClearAdd) return;
            _addCategoryValue = "";
            RepaintGUI();
        }

        #endregion

        #endregion
    }
}
#endif
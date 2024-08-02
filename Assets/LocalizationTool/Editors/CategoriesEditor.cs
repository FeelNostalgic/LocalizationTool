using System.Collections.Generic;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Commons.EditorStrings;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    public class CategoriesEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private string _addCategoryValue;

        private const int MAX_CHARACTERS = 120;

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

        public void ShowLayout()
        {
            ControlFocus(CATEGORY_LABEL_UPPER);

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

            GUILayout.Label(CATEGORY_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(3);

            GUI.SetNextControlName(CATEGORY_LABEL_UPPER);
            _addCategoryValue = EditorGUILayout.TextField(_addCategoryValue, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));

            GUILayout.Space(5);

            if (GUILayout.Button(GetGUIContent(AddIcon, ADD_CATEGORY_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                LocalizationManager.Instance.AddNewCategory(_addCategoryValue, this);
            }

            if (GUI.GetNameOfFocusedControl() == CATEGORY_LABEL_UPPER)
            {
                if (Event.current is { keyCode: (KeyCode.Return or KeyCode.KeypadEnter) })
                {
                    if (!AddActionRunning) LocalizationManager.Instance.AddNewCategory(_addCategoryValue, this);
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
                LocalizationManager.Instance.RefreshCategoriesData();
                LocalizationManager.Log(CATEGORIES_LOADED_LOG);
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(CATEGORIES_LABEL_UPPER, CustomStyles.GetStyle(Enums.CustomStyleName.Header1BoldMiddleCenter20Label));

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
            var height = _feedbackList.ContainsKey(category) ? Height : GUILayout.Height(40 + 20);
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Alpha(Color.cyan, .1f);
            GUILayout.BeginVertical("box", height);

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal(Height);

            GUILayout.Space(10);

            Column1(index, category);

            Column2(index, category);

            Column3(index, category);

            Column4(category);

            Column5(category);

            GUILayout.EndHorizontal();

            Feedback(category);

            GUILayout.EndVertical();
        }

        private void Column1(int index, string category)
        {
            //Number
            GUILayout.BeginVertical("box", GUILayout.Width(35), Height);
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
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(UpIcon, string.Format(MOVE_CATEGORY_UP_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(category)) UpdateIndex(index, index - 1);
            }

            GUILayout.EndVertical();
        }

        private void Column3(int index, string category)
        {
            //Button To move DOWN
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetGUIContent(DownIcon, string.Format(MOVE_CATEGORY_DOWN_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
            {
                if (!LocalizationManager.IsDefaultCategory(category)) UpdateIndex(index, index + 1);
            }

            GUILayout.EndVertical();
        }

        private void Column4(string category)
        {
            // Favourite Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(26), Height);
            GUI.backgroundColor = Colors.DEFAULT;

            GUILayout.FlexibleSpace();
            if (LocalizationManager.IsDefaultCategory(category))
            {
                if (GUILayout.Button(GetGUIContent(LocalizationManager.YellowIcon, string.Format(CATEGORY_IS_DEFAULT_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon))) ;
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(FavouriteIcon, string.Format(MAKE_CATEGORY_DEFAULT_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    LocalizationManager.Instance.ChangeDefaultCategory(category);
                    LocalizationManager.Log(string.Format(CATEGORY_DEFAULT_LOG, category));
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
            var tempValue = EditorGUILayout.TextField(category, CustomStyles.GetStyle(Enums.CustomStyleName.ValueMiddleLeftTextField), GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck()) UpdateCategory(category, tempValue);

            // Delete Button
            GUI.backgroundColor = Color.clear;
            GUILayout.BeginVertical("box", GUILayout.Width(23));
            GUI.backgroundColor = Colors.DEFAULT;
            if (LocalizationManager.IsDefaultCategory(category))
            {
                if (GUILayout.Button(GetGUIContent(LockIcon, DELETE_CATEGORY_DEFAULT_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon))) ;
            }
            else
            {
                if (GUILayout.Button(GetGUIContent(DeleteIcon, string.Format(DELETE_CATEGORY_BUTTON_TOOLTIP, category)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                {
                    if (LocalizationManager.Configuration.CategoryDeleteConfirmation)
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
            LocalizationManager.Instance.RemoveCategory(category);
            LocalizationManager.Log(string.Format(DELETED_CATEGORY_LOG, category));
        }

        private void UpdateCategory(string oldCategory, string newCategory)
        {
            if (oldCategory.Equals(newCategory)) return;

            if (!_feedbackList.ContainsKey(oldCategory)) _feedbackList.Add(oldCategory, "");

            if (newCategory.IsEmpty())
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, EMPTY_CATEGORY_FEEDBACK_LABEL, delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            if (newCategory.Contains(" "))
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, SPACES_CATEGORY_FEEDBACK_LABEL, delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            if (newCategory.Length > MAX_CHARACTERS)
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, string.Format(CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL, MAX_CHARACTERS), delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            if (LocalizationManager.Categories.Contains(newCategory))
            {
                ControlFeedbackLabelInRow(delegate(string s) { _feedbackList[oldCategory] = s; }, string.Format(CATEGORY_EXIST_FEEDBACK_LABEL, newCategory), delegate { _feedbackList.Remove(oldCategory); });
                return;
            }

            LocalizationManager.Instance.ChangeCategoryName(oldCategory, newCategory);

            _feedbackList.Remove(oldCategory);
        }

        private static void UpdateIndex(int oldIndex, int newIndex)
        {
            LocalizationManager.Instance.ChangeCategoryIndex(oldIndex, newIndex);
        }

        public override void ClearAddTextField()
        {
            if (!LocalizationManager.Configuration.CategoryClearAdd) return;
            _addCategoryValue = "";
            RepaintGUI();
        }

        #endregion

        #endregion
    }
#endif
}
using System.Threading.Tasks;
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

            GUILayout.Label("CATEGORIES", SubSectionHeaderStyle());
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

            GUILayout.Label("CATEGORIES", HeaderStyle());

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
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            
            //Number
            var tempIndex = EditorGUILayout.IntField(index, IntFieldValueStyle(), GUILayout.Width(35), GUILayout.Height(24));
            UpdateIndex(index, tempIndex);
            
            GUILayout.Space(5);
            
            //Buttons To move
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrollup", "Locked"), GUILayout.Width(25), GUILayout.Height(24)))
            {
                // TODO
                UpdateIndex(index, index - 2);
            }
            
            GUILayout.Space(3);
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrolldown", "Locked"), GUILayout.Width(25), GUILayout.Height(24)))
            {
                // TODO
                UpdateIndex(index, index + 2);
            }
            
            GUILayout.Space(12);

            var tempValue = EditorGUILayout.TextField(category, TextFieldValueStyle(), MinHeightOption(24));
            if (tempValue.Length > MAX_CHARACTERS) tempValue = tempValue[..MAX_CHARACTERS];
            UpdateCategory(category, tempValue);

            GUILayout.Space(10);

            if (category.Equals(NONE))
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_AssemblyLock", "Locked"), CenterButtonWithIconStyle()))
                {
                    if (EditorUtility.DisplayDialog("Warning", $"You can not remove None category", "Ok"))
                    {
                    }
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete"), CenterButtonWithIconStyle()))
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

            GUILayout.Space(10);

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
            if(oldIndex == 1) return;
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
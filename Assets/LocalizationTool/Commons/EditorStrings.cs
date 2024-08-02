namespace LocalizationTool.Commons
{
    public abstract class EditorStrings
    {
        #region LABELS

        public const string KEY_LABEL_UPPER = "KEY";
        public const string CATEGORY_LABEL_UPPER = "CATEGORY";
        public const string VALUE_LABEL_UPPER = "VALUE";

        public const string CATEGORIES_LABEL_UPPER = "CATEGORIES";

        #endregion

        #region TOOLTIPS

        #region Commons

        public const string RELOAD_BUTTON_TOOLTIP = "Reload data";

        #endregion

        #region Dictionary

        public const string ADD_KEY_BUTTON_TOOLTIP = "Add new key to dictionary";
        public const string KEY_SELECTABLE_LABEL_TOOLTIP = "Click to copy key to clipboard";
        public const string COPY_KEY_TOOLTIP = "Key '{0}' copied to clipboard";
        public const string OPEN_TEXT_EDITOR_BUTTON_TOOLTIP = "Open rich text editor";
        public const string DELETE_KEY_BUTTON_TOOLTIP = "Delete key '{0}'";

        #endregion

        #region Languages

        #endregion

        #region Categories

        public const string ADD_CATEGORY_BUTTON_TOOLTIP = "Add new category";
        public const string MOVE_CATEGORY_UP_BUTTON_TOOLTIP = "Move '{0}' Up";
        public const string MOVE_CATEGORY_DOWN_BUTTON_TOOLTIP = "Move '{0}' Down";
        public const string CATEGORY_IS_DEFAULT_BUTTON_TOOLTIP = "'{0}' is default category";
        public const string MAKE_CATEGORY_DEFAULT_BUTTON_TOOLTIP = "Make '{0}' default";
        public const string DELETE_CATEGORY_DEFAULT_TOOLTIP = "Default category cannot be removed";
        public const string DELETE_CATEGORY_BUTTON_TOOLTIP = "Delete category '{0}'";

        #endregion

        #region Configuration

        #endregion

        #endregion

        #region Display Dialog
        
        public const string DELETE_DIALOG_TITLE = "Confirm Delete";
        public const string DELETE_DIALOG_QUESTION = "Are you sure you want to delete '{0}'?";
        public const string DIALOG_OPTION_CANCEL = "Cancel";
        public const string DIALOG_OPTION_DELETE = "Delete";
        
        #endregion

        #region LOG

        public const string DICTIONARY_LOADED_LOG = "Dictionary loaded";
        public const string EDITING_KEY_LOG = "Editing key '{0}'";
        public const string DELETED_KEY_LOG = "Key '{0}' deleted";

        public const string CATEGORIES_LOADED_LOG = "Categories loaded";
        public const string CATEGORY_DEFAULT_LOG = "Category '{0}' is now default";
        public const string DELETED_CATEGORY_LOG = "Category '{0}' deleted";

        #endregion

        #region FEEDBACK LABEL

        public const string EMPTY_CATEGORY_FEEDBACK_LABEL = "Category cannot be an empty value";
        public const string SPACES_CATEGORY_FEEDBACK_LABEL = "Category cannot contain spaces";
        public const string CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL = "The maximum number of characters({0}) has been exceeded";
        public const string CATEGORY_EXIST_FEEDBACK_LABEL = "Category '{0}' value already exists";

        #endregion
    }
}
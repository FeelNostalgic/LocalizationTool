namespace LocalizationTool.Commons
{
    public abstract class EditorStrings
    {
        #region LABELS

        public const string KEY_LABEL_UPPER = "KEY";
        public const string CATEGORY_LABEL_UPPER = "CATEGORY";
        public const string VALUE_LABEL_UPPER = "VALUE";

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

        #endregion

        #region Configuration

        #endregion

        #endregion

        #region Display Dialog

        #region Commons

        public const string DIALOG_OPTION_CANCEL = "Cancel";

        #endregion

        #region Dictionary

        public const string DELETE_KEY_DIALOG_TITLE = "Confirm Delete";
        public const string DELETE_KEY_DIALOG_QUESTION = "Are you sure you want to delete '{0}'?";
        public const string DELETE_KEY_DIALOG_OPTION_DELETE = "Delete";

        #endregion

        #endregion

        #region LOG

        public const string DICTIONARY_LOADED_LOG = "Dictionary loaded";
        public const string EDITING_KEY_LOG = "Editing key '{0}'";
        public const string DELETED_KEY_LOG = "Key '{0}' deleted";

        #endregion
    }
}
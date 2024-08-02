namespace LocalizationTool.Commons
{
    public abstract class EditorStrings
    {
        #region LABELS

        public const string MAIN_WINDOW_LABEL = "Localization Tool";
        public const string DICTIONARY_TOOLBAR_LABEL = "Dictionary";
        public const string LANGUAGES_TOOLBAR_LABEL = "Languages";
        public const string CATEGORIES_TOOLBAR_LABEL = "Categories";
        public const string CONFIGURATION_TOOLBAR_LABEL = "Configuration";

        public const string KEY_LABEL_UPPER = "KEY";
        public const string CATEGORY_LABEL_UPPER = "CATEGORY";
        public const string VALUE_LABEL_UPPER = "VALUE";
        public const string LANGUAGE_LABEL_UPPER = "VALUE";

        public const string CATEGORIES_LABEL_UPPER = "CATEGORIES";
        public const string LANGUAGES_LABEL_UPPER = "LANGUAGES";

        public const string CONFIGURATION_LABEL_UPPER = "CONFIGURATION";
        public const string CONFIGURATION_DELETE_SECTION_LABEL = "Delete Confirmation";
        public const string CONFIGURATION_DELETE_DICTIONARY_LABEL = " Show delete confirmation in DICTIONARY";
        public const string CONFIGURATION_DELETE_LANGUAGES_LABEL = " Show delete confirmation in LANGUAGES";
        public const string CONFIGURATION_DELETE_CATEGORIES_LABEL = " Show delete confirmation in CATEGORIES";
        public const string CONFIGURATION_CLEAR_ADD_SECTION_LABEL = "Clear Add Text Field";
        public const string CONFIGURATION_CLEAR_ADD_DICTIONARY_LABEL = " Clear key when adding in DICTIONARY";
        public const string CONFIGURATION_CLEAR_ADD_LANGUAGES_LABEL = " Clear language when adding in LANGUAGES";
        public const string CONFIGURATION_CLEAR_ADD_CATEGORIES_LABEL = " Clear category when adding in CATEGORIES";
        public const string CONFIGURATION_SEARCH_SECTION_LABEL = "Search";
        public const string CONFIGURATION_LOGS_SECTION_LABEL = "Logs";
        public const string CONFIGURATION_LOGS_TOGGLE_LABEL = "Show logs in Console";
        public const string CONFIGURATION_INFO_SECTION_LABEL = "Information";
        public const string CONFIGURATION_INFO_README_BUTTON_LABEL = "Readme";
        public const string CONFIGURATION_INFO_DOCUMENTATION_BUTTON_LABEL = "Documentation";
        public const string CONFIGURATION_INFO_LICENSE_BUTTON_LABEL = "License";

        public const string EXPORT_LABEL_UPPER = "EXPORT";
        public const string IMPORT_LABEL_UPPER = "IMPORT";
        public const string CSV_LABEL_UPPER = "CSV";
        public const string JSON_LABEL_UPPER = "JSON";
        public const string XML_LABEL_UPPER = "XML";
        public const string CSV_LABEL_LOWER = "csv";
        public const string JSON_LABEL_LOWER = "json";
        public const string XML_LABEL_LOWER = "xml";
        public const string DEFAULT_FILE_NAME = "LocalizationData{0}";
        public const string CSV_WARNING_LABEL = "Line breaks are removed when exporting to CSV";
        public const string IMPORT_WINDOW_LABEL = "Import {0}";
        public const string IMPORT_WINDOW_LANGUAGE_LABEL = "Import {0} {1}";

        public const string IMPORT_STATUS_LANGUAGES = "Importing languages...";
        public const string IMPORT_STATUS_KEYS = "Importing keys...";
        public const string IMPORT_RESULT_SUCCESS = "File has been imported successfully!";
        public const string IMPORT_LANGUAGES_RESULT = "{0} language imported";
        public const string IMPORT_CATEGORIES_RESULT = "{0} new categories imported";
        public const string IMPORT_KEYS_RESULT = "{0} keys imported";
        public const string IMPORT_PROGRESS_KEY_CATEGORY = "Key '{0}' - '{1}' imported";
        public const string IMPORT_PROGRESS_LANGUAGE = "Language '{0}' imported";

        public const string CLOSE_BUTTON_LABEL = "Close";

        public const string LANGUAGE_MANAGER_WINDOW_TITLE_LABEL = "{0} Manager Editor";
        public const string LANGUAGE_MANAGER_TITLE_LABEL = "Manage {0}";

        #endregion

        #region TOOLTIPS

        #region Commons

        public const string RELOAD_BUTTON_TOOLTIP = "Reload data";
        public const string MOVE_UP_BUTTON_TOOLTIP = "Move '{0}' Up";
        public const string MOVE_DOWN_BUTTON_TOOLTIP = "Move '{0}' Down";

        #endregion

        #region Dictionary

        public const string ADD_KEY_BUTTON_TOOLTIP = "Add new key to dictionary";
        public const string KEY_SELECTABLE_LABEL_TOOLTIP = "Click to copy key to clipboard";
        public const string COPY_KEY_TOOLTIP = "Key '{0}' copied to clipboard";
        public const string OPEN_TEXT_EDITOR_BUTTON_TOOLTIP = "Open rich text editor";
        public const string DELETE_KEY_BUTTON_TOOLTIP = "Delete key '{0}'";

        #endregion

        #region Languages

        public const string ADD_LANGUAGE_BUTTON_TOOLTIP = "Add new language";  
        public const string LANGUAGE_IS_FAVOURITE_BUTTON_TOOLTIP = "'{0}' is favourite language";
        public const string MAKE_LANGUAGE_FAVOURITE_BUTTON_TOOLTIP = "Make '{0}' favourite";
        public const string OPEN_MANAGE_MENU_BUTTON_TOOLTIP = "Open Manage Menu";
        public const string DELETE_LANGUAGE_FAVOURITE_TOOLTIP = "Favourite language cannot be deleted";
        public const string DELETE_LANGUAGE_TOOLTIP = "Delete language '{0}'";

        #endregion

        #region Categories

        public const string ADD_CATEGORY_BUTTON_TOOLTIP = "Add new category";
        public const string CATEGORY_IS_DEFAULT_BUTTON_TOOLTIP = "'{0}' is default category";
        public const string MAKE_CATEGORY_DEFAULT_BUTTON_TOOLTIP = "Make '{0}' default";
        public const string DELETE_CATEGORY_DEFAULT_TOOLTIP = "Default category cannot be deleted";
        public const string DELETE_CATEGORY_BUTTON_TOOLTIP = "Delete category '{0}'";

        #endregion

        #region Configuration

        public const string CONFIGURATION_DELETE_SECTION_TOOLTIP = "Show confirmation window when deleting items";
        public const string CONFIGURATION_CLEAR_ADD_SECTION_TOOLTIP = "When adding a new item, the text field will be cleared";
        public const string CONFIGURATION_SEARCH_SECTION_TOOLTIP = "Select how to search for items";
        public const string CONFIGURATION_INFO_SECTION_TOOLTIP = "Information about tool";
        public const string CONFIGURATION_INFO_README_BUTTON_TOOLTIP = "Open Readme";
        public const string CONFIGURATION_INFO_DOCUMENTATION_BUTTON_TOOLTIP = "Open Documentation";
        public const string CONFIGURATION_INFO_LICENSE_BUTTON_TOOLTIP = "Open License";

        public const string EXPORT_BUTTON_TOOLTIP = "Save {0} file";
        public const string IMPORT_BUTTON_TOOLTIP = "Load {0} file";

        #endregion

        #endregion

        #region Display Dialog

        public const string DELETE_DIALOG_TITLE = "Confirm Delete";
        public const string DELETE_DIALOG_MESSAGE = "Are you sure you want to delete '{0}'?";
        public const string DIALOG_OPTION_CANCEL = "Cancel";
        public const string DIALOG_OPTION_DELETE = "Delete";

        public const string FILE_SAVED_DIALOG_TITLE = "File Saved";
        public const string FILE_SAVED_DIALOG_MESSAGE = "File has been saved successfully!";
        public const string DIALOG_OK_OPTION = "OK";

        #endregion

        #region LOG

        public const string DICTIONARY_LOADED_LOG = "Dictionary loaded";
        public const string EDITING_KEY_LOG = "Editing key '{0}'";
        public const string DELETED_KEY_LOG = "Key '{0}' deleted";

        public const string CATEGORIES_LOADED_LOG = "Categories loaded";
        public const string CATEGORY_DEFAULT_LOG = "Category '{0}' is now default";
        public const string DELETED_CATEGORY_LOG = "Category '{0}' deleted";

        public const string LANGUAGES_LOADED_LOG = "Languages loaded";
        public const string LANGUAGE_FAVOURITE_LOG = "Language '{0}' is now favorite";
        public const string DELETED_LANGUAGE_LOG = "Language '{0}' deleted";

        #endregion

        #region FEEDBACK LABEL

        public const string EMPTY_CATEGORY_FEEDBACK_LABEL = "Category cannot be an empty value";
        public const string SPACES_CATEGORY_FEEDBACK_LABEL = "Category cannot contain spaces";
        public const string CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL = "The maximum number of characters({0}) has been exceeded";
        public const string CATEGORY_EXIST_FEEDBACK_LABEL = "Category '{0}' value already exists";

        public const string EMPTY_LANGUAGE_FEEDBACK_LABEL = "Language cannot be an empty value";
        public const string SPACES_LANGUAGE_FEEDBACK_LABEL = "Language cannot contain spaces";
        public const string CHARACTERS_NUMBER_LANGUAGE_FEEDBACK_LABEL = "The maximum number of characters({0}) has been exceeded";
        public const string LANGUAGE_EXIST_FEEDBACK_LABEL = "Language {0} value already exists";

        #endregion
    }
}
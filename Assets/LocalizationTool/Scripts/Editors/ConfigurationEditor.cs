using System;
using System.IO;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using LocalizationTool.Scripts.Serializer;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.EditorPaths;

namespace LocalizationTool.Scripts.Editors
{
    public class ConfigurationEditor : EditorWindowAbstract
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private bool _toggleAllDeleteConfirmation;
        private bool _toggleAllClearAdd;

        private int _searchTypeIndex;

        private Enums.ExportImportMethods _exportMethod;
        private Enums.ExportImportMethods _importMethod;

        private int _selectedCsvSeparatorIndexForExport;
        private int _selectedCsvSeparatorIndexForImport;
        
        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.5f;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public override void ShowLayout()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            Title();

            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal();

            OptionsSection();

            ShowVerticalLine(5);

            ExportImportSection();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        
        #endregion

        #region PRIVATE METHODS

        private static void Title()
        {
            GUILayout.Space(15);

            ShowHeader1(CONFIGURATION_LABEL_UPPER);

            GUILayout.Space(10);
        }

        #region CONFIGURATION SECTION

        private void OptionsSection()
        {
            GUILayout.BeginHorizontal(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT, WindowSize.x)));

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(MinWidthOption(300));

            DeleteConfirmationSection();

            ClearAddSection();

            SearchSection();

            LogsSection();

            ReadmeSection();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void DeleteConfirmationSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));
            GUILayout.Space(10);

            ShowSubHeader(CONFIGURATION_DELETE_SECTION_LABEL, CONFIGURATION_DELETE_SECTION_TOOLTIP);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            if (LocalizationManager.Configuration.dictionaryDeleteConfirmation
                && LocalizationManager.Configuration.languageDeleteConfirmation
                && LocalizationManager.Configuration.categoryDeleteConfirmation) _toggleAllDeleteConfirmation = true;
            else _toggleAllDeleteConfirmation = false;

            var tempToogle = EditorGUILayout.Toggle(_toggleAllDeleteConfirmation, GUILayout.Height(28));
            UpdateAllDeleteConfirmation(_toggleAllDeleteConfirmation, tempToogle);

            ToggleLeft(LocalizationManager.Configuration.dictionaryDeleteConfirmation, CONFIGURATION_DELETE_DICTIONARY_LABEL,
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Dictionary); });

            ToggleLeft(LocalizationManager.Configuration.languageDeleteConfirmation, CONFIGURATION_DELETE_LANGUAGES_LABEL,
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Language); });

            ToggleLeft(LocalizationManager.Configuration.categoryDeleteConfirmation, CONFIGURATION_DELETE_CATEGORIES_LABEL,
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Category); });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void ClearAddSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));

            ShowSubHeader(CONFIGURATION_CLEAR_ADD_SECTION_LABEL, CONFIGURATION_CLEAR_ADD_SECTION_TOOLTIP);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            if (LocalizationManager.Configuration.dictionaryClearAdd
                && LocalizationManager.Configuration.languageClearAdd
                && LocalizationManager.Configuration.categoryClearAdd) _toggleAllClearAdd = true;
            else _toggleAllClearAdd = false;

            var tempToggle = EditorGUILayout.Toggle(_toggleAllClearAdd, GUILayout.Height(28));
            UpdateAllClearAdd(_toggleAllClearAdd, tempToggle);

            ToggleLeft(LocalizationManager.Configuration.dictionaryClearAdd, CONFIGURATION_CLEAR_ADD_DICTIONARY_LABEL,
                delegate(bool b) { UpdateClearAdd(b, Enums.GUIWindow.Dictionary); });

            ToggleLeft(LocalizationManager.Configuration.languageClearAdd, CONFIGURATION_CLEAR_ADD_LANGUAGES_LABEL,
                delegate(bool b) { UpdateClearAdd(b, Enums.GUIWindow.Language); });

            ToggleLeft(LocalizationManager.Configuration.categoryClearAdd, CONFIGURATION_CLEAR_ADD_CATEGORIES_LABEL,
                delegate(bool b) { UpdateClearAdd(b, Enums.GUIWindow.Category); });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void SearchSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));

            ShowSubHeader(CONFIGURATION_SEARCH_SECTION_LABEL, CONFIGURATION_SEARCH_SECTION_TOOLTIP);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.searchTypeIndex, Enums.SEARCH_TYPE, CustomStyles.GetStyle(Enums.CustomStyleName.SearchTypePopup));
            UpdateSearchType();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void LogsSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));

            ShowSubHeader(CONFIGURATION_LOGS_SECTION_LABEL);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            ToggleLeft(LocalizationManager.Configuration.showLogsInConsole, CONFIGURATION_LOGS_TOGGLE_LABEL, UpdateShowLogs);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void ReadmeSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));

            ShowSubHeader(CONFIGURATION_INFO_SECTION_LABEL, CONFIGURATION_INFO_SECTION_TOOLTIP);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            if (GUILayout.Button(new GUIContent(CONFIGURATION_INFO_README_BUTTON_LABEL, CONFIGURATION_INFO_README_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: fill readme (and change title?)
                InfoEditor.ShowWindow(CONFIGURATION_INFO_README_BUTTON_LABEL, GetInfoText(README_PATH));
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent(CONFIGURATION_INFO_DOCUMENTATION_BUTTON_LABEL, CONFIGURATION_INFO_DOCUMENTATION_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: Open documentation window or link / open pdf
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent(CONFIGURATION_INFO_LICENSE_BUTTON_LABEL, CONFIGURATION_INFO_LICENSE_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: fill license (and change title?)
                InfoEditor.ShowWindow(CONFIGURATION_INFO_LICENSE_BUTTON_LABEL, GetInfoText(LICENSE_PATH));
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion

        #region COMMONS

        // ReSharper disable once RedundantAssignment
        private static void ToggleLeft(bool value, string label, Action<bool> action)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Toggle(value, GUILayout.Height(28), GUILayout.Width(15));
            if (EditorGUI.EndChangeCheck()) action?.Invoke(newValue);
            EditorGUILayout.LabelField(label, CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationToggleLabel), GUILayout.Height(28));
            GUILayout.EndHorizontal();
        }

        private static string GetInfoText(string path)
        {
            using var reader = new StreamReader(path);
            return reader.ReadToEnd();
        }

        #endregion

        #region EXPORT SECTION

        private void ExportImportSection()
        {
            GUILayout.BeginVertical(); // 0

            {
                // Export
                GUILayout.BeginVertical(GUILayout.Height(200)); // 1

                GUILayout.BeginHorizontal(); // 2

                GUILayout.FlexibleSpace(); //To center section
                GUILayout.BeginVertical(GUILayout.Width(300)); // 3
                ShowSectionHeader(EXPORT_LABEL_UPPER);

                GUILayout.Space(10);
                ShowExportSection();

                GUILayout.EndVertical(); // 3
                GUILayout.FlexibleSpace(); //To center section

                GUILayout.EndHorizontal(); // 2

                GUILayout.EndVertical(); // 1
            }
            ShowHorizontalLine(5);
            {
                // Import
                GUILayout.BeginVertical(GUILayout.Height(200)); // 4

                GUILayout.BeginHorizontal(); // 5
                GUILayout.FlexibleSpace(); //To center section

                GUILayout.BeginVertical(GUILayout.Width(300)); // 6
                ShowSectionHeader(IMPORT_LABEL_UPPER);

                GUILayout.Space(10);
                ShowImportSection();

                GUILayout.EndVertical(); // 6

                GUILayout.FlexibleSpace(); //To center section
                GUILayout.EndHorizontal(); // 5

                GUILayout.EndVertical(); // 4
            }

            GUILayout.EndVertical(); // 0
        }

        private void ShowExportSection()
        {
            GUILayout.BeginVertical(GUILayout.Height(50));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(8);
            _exportMethod = (Enums.ExportImportMethods)EditorGUILayout.EnumPopup(_exportMethod, CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportEnumPopup), GUILayout.Width(125));

            switch (_exportMethod)
            {
                case Enums.ExportImportMethods.CSV:
                    GUILayout.Space(5);

                    _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, CsvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvPopup),
                        GUILayout.Width(55));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel(string.Format(EXPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER), "",
                            string.Format(DEFAULT_FILE_NAME, CSV_LABEL_UPPER), CSV_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = LocalizationManager.BuildCSV(CsvSeparators[_selectedCsvSeparatorIndexForExport]);
                            SaveFile(path, fileContent);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label(WarningIcon);
                    var style = new GUIStyle(GUI.skin.label) { wordWrap = true, };
                    GUILayout.Label(CSV_WARNING_LABEL, style);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel(string.Format(EXPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER), "",
                            string.Format(DEFAULT_FILE_NAME, JSON_LABEL_UPPER), JSON_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = LocalizationManager.BuildSerializedData(new UnityJsonSerializer());
                            SaveFile(path, fileContent);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel(XML_LABEL_UPPER, "",
                            string.Format(DEFAULT_FILE_NAME, XML_LABEL_UPPER), XML_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = LocalizationManager.BuildSerializedData(new XmlSerializerService());
                            SaveFile(path, fileContent);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUILayout.EndVertical();
        }

        private static void SaveFile(string path, string fileContent)
        {
            // Create and save the file
            LocalizationManager.SaveFile(path, fileContent, FILE_SAVED_DIALOG_TITLE, FILE_SAVED_DIALOG_MESSAGE, DIALOG_OK_OPTION);
        }

        #endregion

        #region IMPORT SECTION

        private void ShowImportSection()
        {
            GUILayout.BeginVertical(GUILayout.Height(50));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(8);
            _importMethod = (Enums.ExportImportMethods)EditorGUILayout.EnumPopup(_importMethod, CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportEnumPopup), GUILayout.Width(125));

            switch (_importMethod)
            {
                case Enums.ExportImportMethods.CSV:
                    GUILayout.Space(5);

                    _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, CsvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvPopup),
                        GUILayout.Width(55));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER), "", CSV_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LABEL, CSV_LABEL_UPPER));

                            EditorCoroutineUtility.StartCoroutine(LocalizationManager.ImportCSVCoroutine(path, CsvSeparators[_selectedCsvSeparatorIndexForImport],progressWindow), progressWindow);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER), "", JSON_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LABEL, JSON_LABEL_UPPER));

                            EditorCoroutineUtility.StartCoroutine(LocalizationManager.ImportSerializedDataCoroutine(path, new UnityJsonSerializer(), progressWindow), progressWindow);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER), "", XML_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LABEL, XML_LABEL_UPPER));
                            EditorCoroutineUtility.StartCoroutine(LocalizationManager.ImportSerializedDataCoroutine(path, new XmlSerializerService(), progressWindow), progressWindow);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUILayout.EndVertical();
        }

        #endregion

        #region UPDATES

        private void UpdateAllDeleteConfirmation(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            _toggleAllDeleteConfirmation = newValue;
            UpdateDeleteConfirmation(newValue, Enums.GUIWindow.Dictionary);

            UpdateDeleteConfirmation(newValue, Enums.GUIWindow.Language);

            UpdateDeleteConfirmation(newValue, Enums.GUIWindow.Category);
        }

        private static void UpdateDeleteConfirmation(bool newValue, Enums.GUIWindow window)
        {
            switch (window)
            {
                case Enums.GUIWindow.Dictionary:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(newValue, window);
                    break;
                case Enums.GUIWindow.Language:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(newValue, window);
                    break;
                case Enums.GUIWindow.Category:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(newValue, window);
                    break;
            }
        }

        private void UpdateAllClearAdd(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            _toggleAllClearAdd = newValue;
            UpdateClearAdd(newValue, Enums.GUIWindow.Dictionary);

            UpdateClearAdd(newValue, Enums.GUIWindow.Language);

            UpdateClearAdd(newValue, Enums.GUIWindow.Category);
        }

        private static void UpdateClearAdd(bool newValue, Enums.GUIWindow window)
        {
            switch (window)
            {
                case Enums.GUIWindow.Dictionary:
                    LocalizationManager.Instance.UpdateClearAdd(newValue, window);
                    break;
                case Enums.GUIWindow.Language:
                    LocalizationManager.Instance.UpdateClearAdd(newValue, window);
                    break;
                case Enums.GUIWindow.Category:
                    LocalizationManager.Instance.UpdateClearAdd(newValue, window);
                    break;
            }
        }

        private void UpdateSearchType()
        {
            LocalizationManager.Instance.UpdateSearchType(_searchTypeIndex);
        }

        private static void UpdateShowLogs(bool newValue)
        {
            LocalizationManager.Instance.UpdateShowLog(newValue);
        }

        #endregion
        
        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Data.Templates;
using LocalizationTool.ExportSerializer;
using LocalizationTool.Manager;
using LocalizationTool.Serializer;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Commons.EditorStrings;

namespace LocalizationTool.Editors
{
    public class ConfigurationEditor : LocalizationEditor
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

        private const string README_PATH = "Assets/LocalizationTool/Data/DoNotTouch/Info/Readme.txt";
        private const string LICENSE_PATH = "Assets/LocalizationTool/Data/DoNotTouch/Info/License.txt";

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.5f;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public void ShowLayout()
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
            GUILayout.BeginHorizontal(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));

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

            if (LocalizationManager.Configuration.DictionaryDeleteConfirmation
                && LocalizationManager.Configuration.LanguageDeleteConfirmation
                && LocalizationManager.Configuration.CategoryDeleteConfirmation) _toggleAllDeleteConfirmation = true;
            else _toggleAllDeleteConfirmation = false;

            var tempToogle = EditorGUILayout.Toggle(_toggleAllDeleteConfirmation, GUILayout.Height(28));
            UpdateAllDeleteConfirmation(_toggleAllDeleteConfirmation, tempToogle);

            ToggleLeft(LocalizationManager.Configuration.DictionaryDeleteConfirmation, CONFIGURATION_DELETE_DICTIONARY_LABEL,
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Dictionary); });

            ToggleLeft(LocalizationManager.Configuration.LanguageDeleteConfirmation, CONFIGURATION_DELETE_LANGUAGES_LABEL,
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Language); });

            ToggleLeft(LocalizationManager.Configuration.CategoryDeleteConfirmation, CONFIGURATION_DELETE_CATEGORIES_LABEL,
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

            if (LocalizationManager.Configuration.DictionaryClearAdd
                && LocalizationManager.Configuration.LanguageClearAdd
                && LocalizationManager.Configuration.CategoryClearAdd) _toggleAllClearAdd = true;
            else _toggleAllClearAdd = false;

            var tempToogle = EditorGUILayout.Toggle(_toggleAllClearAdd, GUILayout.Height(28));
            UpdateAllClearAdd(_toggleAllClearAdd, tempToogle);

            ToggleLeft(LocalizationManager.Configuration.DictionaryClearAdd, CONFIGURATION_CLEAR_ADD_DICTIONARY_LABEL,
                delegate(bool b) { UpdateClearAdd(b, Enums.GUIWindow.Dictionary); });

            ToggleLeft(LocalizationManager.Configuration.LanguageClearAdd, CONFIGURATION_CLEAR_ADD_LANGUAGES_LABEL,
                delegate(bool b) { UpdateClearAdd(b, Enums.GUIWindow.Language); });

            ToggleLeft(LocalizationManager.Configuration.CategoryClearAdd, CONFIGURATION_CLEAR_ADD_CATEGORIES_LABEL,
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

            _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.SearchTypeIndex, Enums.SEARCH_TYPE, CustomStyles.GetStyle(Enums.CustomStyleName.SearchTypePopup));
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

            ToggleLeft(LocalizationManager.Configuration.ShowLogsInConsole, CONFIGURATION_LOGS_TOGGLE_LABEL, UpdateShowLogs);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void ReadmeSection()
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
                            var fileContent = BuildCSV();
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
                            var fileContent = BuildSerializedData(new UnityJsonSerializer());
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
                            var fileContent = BuildSerializedData(new XmlSerializerService());
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

                            EditorCoroutineUtility.StartCoroutine(ImportCSVCoroutine(path, progressWindow), progressWindow);
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

                            EditorCoroutineUtility.StartCoroutine(ImportSerializedDataCoroutine(path, new UnityJsonSerializer(), progressWindow), progressWindow);
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
                            // TODO
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LABEL, XML_LABEL_UPPER));
                            EditorCoroutineUtility.StartCoroutine(ImportSerializedDataCoroutine(path, new UnityJsonSerializer(), progressWindow), progressWindow);
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

        #region EXPORTS

        private string BuildCSV()
        {
            var serializer = new CSV_Serializer();
            serializer.SetSeparator(CsvSeparators[_selectedCsvSeparatorIndexForExport]);
            
            serializer.AddTitle(LocalizationManager.ActiveLanguages);
            
            var languageDictionary = new Dictionary<string, int>();
            for (var i = 0; i < LocalizationManager.ActiveLanguages.Count; i++)
            {
                languageDictionary.Add(LocalizationManager.ActiveLanguages[i], i);
            }
            
            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

            foreach (var (key, keyData) in data)
            {
                var items = new List<string>();
                var category = keyData.Category;

                items.Add(key);
                items.Add(category);

                var auxArray = new string[languageDictionary.Count];
                foreach (var (language, value) in keyData.LanguagesData)
                {
                    auxArray[languageDictionary[language]] = value.Replace("\n", " ").Replace("\r", " ");
                }

                items.AddRange(auxArray);

                serializer.AddLine(items);
            }

            return serializer.File();
        }

        private static string BuildSerializedData(ISerializerService serializer)
        {
            var dataToSerialize = new DictionaryTemplate();

            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

            foreach (var (key, keyData) in data)
            {
                var category = keyData.Category;
                var languageValues = keyData.LanguagesData.Select(item => new DictionaryTemplate.LanguageValue { Language = item.Key, Value = item.Value }).ToList();
                dataToSerialize.DictionaryKeyCategoryLanguages.Add(new DictionaryTemplate.KeyCategoryLanguageValues
                {
                    Key = key,
                    Category = category,
                    LanguageValues = languageValues
                });
            }

            return serializer.Serialize(dataToSerialize);
        }

        #endregion

        #region IMPORTS

        private IEnumerator ImportCSVCoroutine(string path, ImportProgressWindow progressWindow)
        {
            var sb = new StringBuilder();

            using var reader = new StreamReader(path);
            var fileInfo = new FileInfo(path);
            var totalBytes = fileInfo.Length;
            long bytesRead = 0;

            progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            var header = reader.ReadLine();
            Debug.Assert(header != null, nameof(header) + " != null");
            bytesRead += header.Length + Environment.NewLine.Length;
            progressWindow.SetProgress((float)bytesRead / totalBytes);

            var separator = CsvSeparators[_selectedCsvSeparatorIndexForImport];
            var languages = header?.Split(separator);

            Debug.Assert(languages != null, nameof(languages) + " != null");
            if (languages.Length < 2)
            {
                var sbResultError = new StringBuilder();
                sbResultError.AppendLine("Error while importing CSV");
                sbResultError.AppendLine($"Separator [ {separator} ] not found");
                progressWindow.Complete(sbResultError.ToString());

                yield break;
            }

            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_LANGUAGES_RESULT, languages.Length - 2));

            var languageOrder = new List<string>();
            for (var i = 2; i < languages.Length; i++)
            {
                languageOrder.Add(languages[i]);
                var loadLanguageTask = LocalizationManager.Instance.ImportLanguage(languages[i]);
                var awaiter = loadLanguageTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;
            }

            progressWindow.SetStatus(IMPORT_STATUS_KEYS);
            var nKeys = 0;
            var nCategories = 0;
            while (!reader.EndOfStream)
            {
                var nextLine = reader.ReadLine();
                Debug.Assert(nextLine != null, nameof(nextLine) + " != null");
                bytesRead += nextLine.Length + Environment.NewLine.Length;
                var lineItems = nextLine?.Split(separator);
                var key = lineItems?[0];
                var category = lineItems?[1];
                var values = new Dictionary<string, string>();

                nKeys++;
                if (!LocalizationManager.Instance.ExistCategory(category)) nCategories++;

                Debug.Assert(lineItems != null, nameof(lineItems) + " != null");
                for (var i = 0; i < lineItems.Length - 2; i++)
                {
                    values.Add(languageOrder[i], lineItems[i + 2]);
                }

                var loadKeyTask = LocalizationManager.Instance.ImportKey(key, category, values);
                var awaiter = loadKeyTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;

                progressWindow.SetProgress((float)bytesRead / totalBytes);
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, key, category));
            }
            
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            progressWindow.Complete(sb.ToString());
        }

        private static IEnumerator ImportSerializedDataCoroutine(string path, ISerializerService serializer, ImportProgressWindow progressWindow)
        {
            var loadFileTask = LocalizationManager.LoadFile<DictionaryTemplate>(path, serializer);
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            var totalItems = data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count + data.DictionaryKeyCategoryLanguages.Count;
            var itemCount = 0f;

            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);

            //Languages
            progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                var task = LocalizationManager.Instance.ImportLanguage(languageValue.Language);
                var awaiterLanguage = task.GetAwaiter();
                while (!awaiterLanguage.IsCompleted) yield return null;
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_LANGUAGE, languageValue.Language));
                progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            var nKeys = 0;
            var nCategories = 0;

            // Keys
            progressWindow.SetStatus(IMPORT_STATUS_KEYS);

            foreach (var keyCategoryLanguage in data.DictionaryKeyCategoryLanguages)
            {
                nKeys++;
                if (!LocalizationManager.Instance.ExistCategory(keyCategoryLanguage.Category)) nCategories++;

                foreach (var awaiterKey in keyCategoryLanguage.LanguageValues
                             .Select(languageValue => LocalizationManager.Instance.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, languageValue.Language, languageValue.Value))
                             .Select(task => task.GetAwaiter()))
                {
                    while (!awaiterKey.IsCompleted) yield return null;

                    yield return null;
                }

                progressWindow.SetProgress(itemCount++ / totalItems);
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, keyCategoryLanguage.Key, keyCategoryLanguage.Category));
                yield return null;
            }
            
            sb.AppendLine(string.Format(IMPORT_LANGUAGES_RESULT, data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count));
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            progressWindow.Complete(sb.ToString());
        }

        #endregion

        #endregion
    }
}
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
using Debug = System.Diagnostics.Debug;

namespace LocalizationTool.Editors
{
    public class ConfigurationEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private bool _toggleAllDeleteConfirmation;

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

        private void Title()
        {
            GUILayout.Space(15);

            ShowHeader1("CONFIGURATION");

            GUILayout.Space(10);
        }

        #region CONFIGURATION SECTION

        private void OptionsSection()
        {
            GUILayout.BeginHorizontal(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(MinWidthOption(300));

            DeleteConfirmationSection();

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
            ShowSubHeader("Delete Confirmation");

            if (LocalizationManager.Configuration.DictionaryDeleteConfirmation
                && LocalizationManager.Configuration.LanguageDeleteConfirmation
                && LocalizationManager.Configuration.CategoryDeleteConfirmation) _toggleAllDeleteConfirmation = true;
            else _toggleAllDeleteConfirmation = false;

            var tempToogle = EditorGUILayout.Toggle(_toggleAllDeleteConfirmation, GUILayout.Height(28));
            UpdateAllDeleteConfirmation(_toggleAllDeleteConfirmation, tempToogle);

            ToggleLeft(LocalizationManager.Configuration.DictionaryDeleteConfirmation, "Show delete confirmation in DICTIONARY",
                delegate(bool b) { UpdateDeleteConfirmation(b,Enums.GUIWindow.Dictionary); });

            ToggleLeft(LocalizationManager.Configuration.LanguageDeleteConfirmation, "Show delete confirmation in LANGUAGES",
                delegate(bool b) { UpdateDeleteConfirmation(b,Enums.GUIWindow.Language); });

            ToggleLeft(LocalizationManager.Configuration.CategoryDeleteConfirmation, "Show delete confirmation in CATEGORIES",
                delegate(bool b) { UpdateDeleteConfirmation(b, Enums.GUIWindow.Category); });

            GUILayout.EndVertical();
        }

        private void SearchSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));
            ShowSubHeader("Search");
            _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.SearchTypeIndex, Enums.SEARCH_TYPE, CustomStyles.GetStyle(Enums.CustomStyleName.SearchTypePopup));
            UpdateSearchType();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        private void LogsSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));
            ShowSubHeader("Logs");
            ToggleLeft(LocalizationManager.Configuration.ShowLogsInConsole, "Show logs in Console", delegate(bool b) { UpdateShowLogs(b); });
            GUILayout.EndVertical();
        }

        private void ReadmeSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            ShowSubHeader("Info");

            if (GUILayout.Button("Readme", CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: fill readme
                InfoEditor.ShowWindow("Readme", GetInfoText(README_PATH));
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Documentation", CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: Open documentation window or link / open pdf
            }

            GUILayout.Space(5);

            if (GUILayout.Button("License", CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationReadmeButton)))
            {
                //TODO: fill license
                InfoEditor.ShowWindow("License", GetInfoText(LICENSE_PATH));
            }

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
                ShowSectionHeader("EXPORT");

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
                ShowSectionHeader("IMPORT");

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

                    _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, _csvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvPopup),
                        GUILayout.Width(55));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save CSV file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save CSV File", "", "LocalizationData.csv", "csv");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildCSV();
                            // Create and save the file
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label(EditorGUIUtility.IconContent("d_console.warnicon.sml"));
                    var style = new GUIStyle(GUI.skin.label) { wordWrap = true, };
                    GUILayout.Label("Line breaks are removed when exporting to CSV", style);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save JSON file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save JSON File", "", "LocalizationData.json", "json");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(new UnityJsonSerializer());
                            // Create and save the file
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save XML file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save XML File", "", "LocalizationData.xml", "xml");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(new XmlSerializerService());
                            // Create and save the file
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
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

                    _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, _csvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvPopup),
                        GUILayout.Width(55));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load CSV file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load CSV File", "", "csv");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow("Import CSV");
                            
                            EditorCoroutineUtility.StartCoroutine(ImportCSVCoroutine(path, progressWindow), progressWindow);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load JSON file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load JSON File", "", "json");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow("Import JSON");

                            EditorCoroutineUtility.StartCoroutine(ImportSerializedDataCoroutine(path, new UnityJsonSerializer(), progressWindow), progressWindow);
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load XML file"), CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationExportImportButton)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load XML File", "", "xml");

                        if (!string.IsNullOrEmpty(path))
                        {
                            // TODO
                            var progressWindow = ImportProgressWindow.OpenWindow("Import XML");
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

            UpdateDeleteConfirmation(newValue,Enums.GUIWindow.Language);
            
            UpdateDeleteConfirmation(newValue,Enums.GUIWindow.Category);
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
            serializer.SetSeparator(_csvSeparators[_selectedCsvSeparatorIndexForExport]);
            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

            var titles = new List<string> { "Key", "Category" };
            titles.AddRange(LocalizationManager.ActiveLanguages);
            var languageDictionary = new Dictionary<string, int>();
            for (var i = 0; i < LocalizationManager.ActiveLanguages.Count; i++)
            {
                languageDictionary.Add(LocalizationManager.ActiveLanguages[i], i);
            }

            serializer.AddLine(titles);

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

        private string BuildSerializedData(ISerializerService serializer)
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
            
            progressWindow.SetStatus("Importing languages...");

            var header = reader.ReadLine();
            Debug.Assert(header != null, nameof(header) + " != null");
            bytesRead += header.Length + Environment.NewLine.Length;
            progressWindow.SetProgress( (float)bytesRead / totalBytes);
            
            var separator = _csvSeparators[_selectedCsvSeparatorIndexForImport];
            var languages = header?.Split(separator);

            Debug.Assert(languages != null, nameof(languages) + " != null");
            if (languages.Length < 2)
            {
                var sbResultError = new StringBuilder();
                sbResultError.AppendLine("Error while importing CSV");
                sbResultError.AppendLine( $"Separator [ {separator} ] not found");
                progressWindow.Complete(sbResultError.ToString());
                
                yield break;
            }
            
            sb.AppendLine("CSV has been imported successfully!");
            sb.AppendLine($"{languages.Length - 2} language imported");

            var languageOrder = new List<string>();
            for (var i = 2; i < languages.Length; i++)
            {
                languageOrder.Add(languages[i]);
                var loadLanguageTask = LocalizationManager.Instance.ImportLanguage(languages[i]);
                var awaiter = loadLanguageTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;
            }
            
            progressWindow.SetStatus("Importing keys...");
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

                var loadKeyTask =  LocalizationManager.Instance.ImportKey(key, category, values);
                var awaiter = loadKeyTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;
                
                progressWindow.SetProgress( (float)bytesRead / totalBytes);
                progressWindow.SetProgressInfo($"Key '{key}' - '{category}' imported");
            }

            var categories = nCategories > 1 ? "Categories" : "Category";
            sb.AppendLine($"{nCategories} new {categories} imported");
            sb.AppendLine($"{nKeys} keys imported");

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
            sb.AppendLine("File has been imported successfully!");

            //Languages
            progressWindow.SetStatus("Importing languages...");

            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                var task = LocalizationManager.Instance.ImportLanguage(languageValue.Language);
                var awaiterLanguage = task.GetAwaiter();
                while (!awaiterLanguage.IsCompleted) yield return null;
                progressWindow.SetProgressInfo($"'{languageValue.Language}' imported");
                progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            var nKeys = 0;
            var nCategories = 0;

            // Keys
            progressWindow.SetStatus("Importing keys...");

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

                progressWindow.SetProgressInfo($"Key '{keyCategoryLanguage.Key}' - '{keyCategoryLanguage.Category}' imported");
                progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            sb.AppendLine($"{data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count} languages imported");
            sb.AppendLine($"{nCategories} new Categories imported");
            sb.AppendLine($"{nKeys} keys imported");

            progressWindow.Complete(sb.ToString());
            //EditorUtility.DisplayDialog("File imported", sb.ToString(), "OK");
        }

        #endregion

        #endregion
    }
}
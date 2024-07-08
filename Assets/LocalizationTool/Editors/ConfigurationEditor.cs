using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Data;
using LocalizationTool.ExportSerializer;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace LocalizationTool.Editors
{
    public class ConfigurationEditor : LocalizationEditor
    {
        #region PUBLIC VARIABLES

        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private bool _toggleAllDeleteConfirmation;
        private bool _dictionaryDeleteConfirmation;
        private bool _languageDeleteConfirmation;
        private bool _categoryDeleteConfirmation;

        private int _searchTypeIndex;

        private bool _showLogs;

        private bool _csvExportSection = true;
        private bool _csvImportSection = true;

        private readonly string[] _csvSeparators = { ",", ";", ".", ":", "|", "=" };
        private int _selectedCsvSeparatorIndexForExport;
        private int _selectedCsvSeparatorIndexForImport;

        private bool _jsonExportSection = true;
        private bool _jsonImportSection = true;

        private bool _xmlExportSection = true;
        private bool _xmlImportSection = true;

        #endregion

        #region DIMENSION VARIABLES

        private const float LEFT_SECTION_WIDTH_PERCENT = 0.5f;

        #endregion

        #endregion

        #region PUBLIC METHODS

        public void ShowLayout()
        {
            GUILayout.BeginVertical();

            Title();

            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal();

            OptionsSection();

            ShowVerticalLine(5);

            ExportSection();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion

        #region PRIVATE METHODS

        private void Title()
        {
            GUILayout.Space(15);

            ShowHeader("CONFIGURATION");

            GUILayout.Space(10);
        }

        private void OptionsSection()
        {
            GUILayout.BeginHorizontal(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            DeleteConfirmationSection();

            SearchSection();

            LogsSection();

            ReadmeSection();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void ExportSection()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginVertical(MinHeightOption(_windowSize.y *0.5f));
            ShowSectionHeader("EXPORT");

            ShowExportCsvSection();
            ShowExportJsonSection();
            ShowExportXMLSection();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            ShowHorizontalLine(5);

            ShowSectionHeader("IMPORT");

            ShowImportCsvSection();
            ShowImportJsonSection();
            ShowImportXmlSection();
            
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void ShowExportCsvSection()
        {
            _csvExportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_csvExportSection, "CSV", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_csvExportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.FlexibleSpace();

                _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, _csvSeparators, SeparatorCSVStyle());
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
                {
                    var path = EditorUtility.SaveFilePanel("Save CSV File", "", "Localization.csv", "csv");

                    if (!string.IsNullOrEmpty(path))
                    {
                        var fileContent = BuildCSV();
                        // Create and save the file
                        SaveFile(path, fileContent);
                    }
                }

                GUILayout.FlexibleSpace();
                
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }
        
        private void ShowExportJsonSection()
        {
            _jsonExportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_jsonExportSection, "JSON", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_jsonExportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
                {
                   //TODO
                }

                GUILayout.FlexibleSpace();
                
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }
        
        private void ShowExportXMLSection()
        {
            _xmlExportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_xmlExportSection, "XML", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_xmlExportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
                {
                    //TODO
                }

                GUILayout.FlexibleSpace();
                
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }
        
        private void ShowImportCsvSection()
        {
            _csvImportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_csvImportSection, "CSV", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_csvImportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.FlexibleSpace();

                _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, _csvSeparators, SeparatorCSVStyle(), MaxWidthOption(110));

                GUILayout.Space(5);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), BiggerButtonWithIconStyle()))
                {
                    var path = EditorUtility.OpenFilePanel("Load CSV File", "", "csv");

                    if (!string.IsNullOrEmpty(path))
                    {
                        ImportCSV(path);
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }

        private void ShowImportJsonSection()
        {
            _jsonImportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_jsonImportSection, "JSON", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_jsonImportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.FlexibleSpace();
                
                GUILayout.Space(5);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), BiggerButtonWithIconStyle()))
                {
                    //TODO
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }

        private void ShowImportXmlSection()
        {
            _xmlImportSection = EditorGUILayout.BeginFoldoutHeaderGroup(_xmlImportSection, "XML", FoldoutHeaderStyle());
            GUILayout.Space(10);
            if (_xmlImportSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.FlexibleSpace();
                
                GUILayout.Space(5);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), BiggerButtonWithIconStyle()))
                {
                    //TODO
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(5);
        }

        private void DeleteConfirmationSection()
        {
            ShowSubHeader("Delete Confirmation");

            if (_dictionaryDeleteConfirmation && _languageDeleteConfirmation && _categoryDeleteConfirmation) _toggleAllDeleteConfirmation = true;
            else _toggleAllDeleteConfirmation = false;

            var tempToogle = EditorGUILayout.Toggle(_toggleAllDeleteConfirmation);
            UpdateAllDeleteConfirmation(_toggleAllDeleteConfirmation, tempToogle);

            _dictionaryDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in DICTIONARY", LocalizationManager.Configuration.DictionaryDeleteConfirmation, MinHeightOption(24));
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Dictionary);

            _languageDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in LANGUAGES", LocalizationManager.Configuration.LanguageDeleteConfirmation, MinHeightOption(24));
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Languages);

            _categoryDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in CATEGORIES", LocalizationManager.Configuration.CategoryDeleteConfirmation, MinHeightOption(24));
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Categories);
        }

        private void SearchSection()
        {
            ShowSubHeader("Search");
            _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.SearchTypeIndex, Enums.SEARCH_TYPE, SearchTypeStyle());
            UpdateSearchType();
            GUILayout.Space(5);
        }

        private void LogsSection()
        {
            ShowSubHeader("Logs");
            _showLogs = EditorGUILayout.ToggleLeft("Show logs in Console", LocalizationManager.Configuration.ShowLogsInConsole, MinHeightOption(24));
            UpdateShowLogs();
        }

        private void ReadmeSection()
        {
            ShowSubHeader("Readme");
            //TODO: poner boton para abrir readme/documentation o link
        }

        #region UPDATES

        private void UpdateAllDeleteConfirmation(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            _toggleAllDeleteConfirmation = newValue;
            _dictionaryDeleteConfirmation = newValue;
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Dictionary);

            _languageDeleteConfirmation = newValue;
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Languages);

            _categoryDeleteConfirmation = newValue;
            UpdateDeleteConfirmation(Enums.GUI_WINDOW.Categories);
        }

        private void UpdateDeleteConfirmation(Enums.GUI_WINDOW window)
        {
            switch (window)
            {
                case Enums.GUI_WINDOW.Dictionary:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(_dictionaryDeleteConfirmation, window);
                    break;
                case Enums.GUI_WINDOW.Languages:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(_languageDeleteConfirmation, window);
                    break;
                case Enums.GUI_WINDOW.Categories:
                    LocalizationManager.Instance.UpdateDeleteConfirmation(_categoryDeleteConfirmation, window);
                    break;
            }
        }

        private void UpdateSearchType()
        {
            LocalizationManager.Instance.UpdateSearchType(_searchTypeIndex);
        }

        private void UpdateShowLogs()
        {
            LocalizationManager.Instance.UpdateShowLog(_showLogs);
        }

        #endregion

        #region EXPORTS

        private string BuildCSV()
        {
            var serializer = new CsvSerializer();
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

        private static async void SaveFile(string path, string fileContent)
        {
            await File.WriteAllTextAsync(path, fileContent);

            EditorUtility.DisplayDialog("File Saved", "File has been saved successfully!", "OK");
        }

        #endregion

        #region IMPORTS

        private async void ImportCSV(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CSV has been imported successfully!");

            using var reader = new StreamReader(path);

            var header = await reader.ReadLineAsync();
            var separator = _csvSeparators[_selectedCsvSeparatorIndexForImport];
            var languages = header.Split(separator);

            sb.AppendLine($"{languages.Length - 2} language imported");

            var languageOrder = new List<string>();
            for (var i = 2; i < languages.Length; i++)
            {
                languageOrder.Add(languages[i]);
                LocalizationManager.Instance.ImportLanguage(languages[i]);
            }

            var nKeys = 0;
            while (!reader.EndOfStream)
            {
                nKeys++;
                var nextLine = await reader.ReadLineAsync();
                var items = nextLine.Split(separator);
                var key = items[0];
                var category = items[1];
                var values = new Dictionary<string, string>();
                
                for (var i = 0; i < items.Length - 2; i++)
                {
                    values.Add(languageOrder[i], items[i+2]);
                }

                LocalizationManager.Instance.ImportKey(key, category, values);

                await Task.Delay(100);
            }

            sb.AppendLine($"{nKeys} keys imported");

            EditorUtility.DisplayDialog("CSV imported", sb.ToString(), "OK");
        }

        #endregion

        #endregion
    }
}
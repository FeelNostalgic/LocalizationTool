using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Data;
using LocalizationTool.Data.Templates;
using LocalizationTool.ExportSerializer;
using LocalizationTool.Manager;
using LocalizationTool.Serializer;
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

        private int _selectedCsvSeparatorIndexForExport;
        private int _selectedCsvSeparatorIndexForImport;

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

        #region CONFIGURATION SECTION

        private void OptionsSection()
        {
            GUILayout.BeginHorizontal(MinWidthOption(GetWidthSize(LEFT_SECTION_WIDTH_PERCENT)));

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(MinWidthOption(275));

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
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
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
            GUILayout.EndVertical();
        }

        private void SearchSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            ShowSubHeader("Search");
            _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.SearchTypeIndex, Enums.SEARCH_TYPE, SearchTypeStyle());
            UpdateSearchType();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        private void LogsSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            ShowSubHeader("Logs");
            _showLogs = EditorGUILayout.ToggleLeft("Show logs in Console", LocalizationManager.Configuration.ShowLogsInConsole, MinHeightOption(24));
            UpdateShowLogs();
            GUILayout.EndVertical();
        }

        private void ReadmeSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            ShowSubHeader("Readme");
            //TODO: poner boton para abrir readme/documentation o link
            GUILayout.EndVertical();
        }

        #endregion

        #region EXPORT SECTION

        private void ExportSection()
        {
            GUILayout.BeginVertical(); // 0

            GUILayout.BeginVertical(MinHeightOption(_windowSize.y * 0.5f)); // 1

            GUILayout.BeginHorizontal(); // 2

            GUILayout.FlexibleSpace(); //To center section
            GUILayout.BeginVertical(GUILayout.Width(300)); // 3
            ShowSectionHeader("EXPORT");

            GUILayout.Space(10);
            ShowExportCsvSection();
            GUILayout.Space(15);
            ShowExportJsonSection();
            GUILayout.Space(15);
            ShowExportXMLSection();

            GUILayout.EndVertical(); // 3
            GUILayout.FlexibleSpace(); //To center section

            GUILayout.EndHorizontal(); // 2

            GUILayout.EndVertical(); // 1

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true)); // 4
            ShowHorizontalLine(5);

            GUILayout.BeginHorizontal(); // 5
            GUILayout.FlexibleSpace(); //To center section

            GUILayout.BeginVertical(GUILayout.Width(200)); // 6
            ShowSectionHeader("IMPORT");

            GUILayout.Space(10);
            ShowImportCsvSection();
            GUILayout.Space(15);
            ShowImportJsonSection();
            GUILayout.Space(15);
            ShowImportXmlSection();

            GUILayout.EndVertical(); // 6

            GUILayout.FlexibleSpace(); //To center section
            GUILayout.EndHorizontal(); // 5

            GUILayout.EndVertical(); // 4

            GUILayout.EndVertical(); // 0
        }

        private void ShowExportCsvSection()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal();

            ShowExportSubHeader("CSV", GUILayout.Width(50), GUILayout.Height(25));

            _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, _csvSeparators, SeparatorCSVStyle());

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save CSV File", "", "LocalizationData.csv", "csv");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildCSV();
                    // Create and save the file
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.Label(EditorGUIUtility.IconContent("d_console.warnicon.sml"));

            GUILayout.Label("Line breaks are removed when exporting to CSV");

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void ShowExportJsonSection()
        {
            GUILayout.BeginHorizontal();

            ShowExportSubHeader("JSON", GUILayout.Width(50), GUILayout.Height(25));

            GUILayout.Space(200);
            GUILayout.FlexibleSpace();


            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save JSON File", "", "LocalizationData.json", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildSerializedData(new UnityJsonSerializer());
                    // Create and save the file
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void ShowExportXMLSection()
        {
            GUILayout.BeginHorizontal();
            ShowExportSubHeader("XML", GUILayout.Width(50), GUILayout.Height(25));

            GUILayout.Space(200);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), BiggerButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save XML File", "", "LocalizationData.xml", "xml");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildSerializedData(new XmlSerializerService());
                    // Create and save the file
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        #endregion

        #region IMPORT SECTION

        private void ShowImportCsvSection()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal();

            ShowExportSubHeader("CSV", GUILayout.Width(50), GUILayout.Height(25));

            _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, _csvSeparators, SeparatorCSVStyle());

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

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

        private void ShowImportJsonSection()
        {
            GUILayout.BeginHorizontal();
            ShowExportSubHeader("JSON", GUILayout.Width(50), GUILayout.Height(25));

            GUILayout.Space(200);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), BiggerButtonWithIconStyle()))
            {
                var path = EditorUtility.OpenFilePanel("Load JSON File", "", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    ImportSerializedData(path, new UnityJsonSerializer());
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void ShowImportXmlSection()
        {
            GUILayout.BeginHorizontal();
            ShowExportSubHeader("XML", GUILayout.Width(50), GUILayout.Height(25));

            GUILayout.Space(200);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Import"), BiggerButtonWithIconStyle()))
            {
                var path = EditorUtility.OpenFilePanel("Load XML File", "", "xml");

                if (!string.IsNullOrEmpty(path))
                {
                    ImportSerializedData(path, new XmlSerializerService());
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        #endregion

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

        private async void ImportCSV(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CSV has been imported successfully!");

            using var reader = new StreamReader(path);

            var header = await reader.ReadLineAsync();
            var separator = _csvSeparators[_selectedCsvSeparatorIndexForImport];
            var languages = header.Split(separator);

            if (languages.Length < 2)
            {
                EditorUtility.DisplayDialog("Error while importing CSV", $"Separator [ {separator} ] not found", "OK");
                return;
            }

            sb.AppendLine($"{languages.Length - 2} language imported");

            var languageOrder = new List<string>();
            for (var i = 2; i < languages.Length; i++)
            {
                languageOrder.Add(languages[i]);
                await LocalizationManager.Instance.ImportLanguage(languages[i]);
            }

            var nKeys = 0;
            while (!reader.EndOfStream)
            {
                nKeys++;
                var nextLine = await reader.ReadLineAsync();
                var lineItems = nextLine.Split(separator);
                var key = lineItems[0];
                var category = lineItems[1];
                var values = new Dictionary<string, string>();

                for (var i = 0; i < lineItems.Length - 2; i++)
                {
                    values.Add(languageOrder[i], lineItems[i + 2]);
                }

                await LocalizationManager.Instance.ImportKey(key, category, values);

                // await Task.Delay(50);
            }

            sb.AppendLine($"{nKeys} keys imported");

            EditorUtility.DisplayDialog("CSV imported", sb.ToString(), "OK");
        }

        private async void ImportSerializedData(string path, ISerializerService serializer)
        {
            var data = await LocalizationManager.LoadFile<DictionaryTemplate>(path, serializer);

            var sb = new StringBuilder();
            sb.AppendLine("File has been imported successfully!");

            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                await LocalizationManager.Instance.ImportLanguage(languageValue.Language);
            }

            var nKeys = 0;
            foreach (var keyCategoryLanguage in data.DictionaryKeyCategoryLanguages)
            {
                nKeys++;
                foreach (var languageValue in keyCategoryLanguage.LanguageValues)
                {
                    await LocalizationManager.Instance.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, languageValue.Language, languageValue.Value);
                }
            }

            sb.AppendLine($"{nKeys} keys imported");

            EditorUtility.DisplayDialog("File imported", sb.ToString(), "OK");
        }

        #endregion

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Data.Templates;
using LocalizationTool.ExportSerializer;
using LocalizationTool.Manager;
using LocalizationTool.Serializer;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
    public class LanguageManagerEditor : LocalizationEditor
    {
        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(250, 300);

        #endregion

        #region PRIVATE VARIABLES

        private string _language;
        private Enums.ExportImportMethods _exportMethod;
        private Enums.ExportImportMethods _importMethod;

        private int _selectedCsvSeparatorIndexForExport;
        private int _selectedCsvSeparatorIndexForImport;

        private ISerializerService _jsonSerializer;
        private ISerializerService _xmlSerializer;

        #endregion

        public static void ShowWindow(string language)
        {
            var window = GetWindow<LanguageManagerEditor>($"{language} Manager Editor");
            window.minSize = WindowSize;
            window.maxSize = WindowSize;
            window._language = language;
            window._jsonSerializer = new UnityJsonSerializer();
            window._xmlSerializer = new XmlSerializerService();
        }

        #region PRIVATE METHODS

        private new void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);
            ShowHeader1($"Manage {_language}");

            ShowHorizontalLine(5);

            ShowHeader2("EXPORT");
            ShowExportSection();

            ShowHeader2("IMPORT");
            ShowImportSection();
            
            GUILayout.EndVertical();
        }

        private void ShowExportSection()
        {
            GUILayout.BeginVertical(GUILayout.Height(40));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(8);
            _exportMethod = (Enums.ExportImportMethods)EditorGUILayout.EnumPopup(_exportMethod, CustomStyles.GetStyle(Enums.CustomStyleName.LanguageManageEditorEnumPopup), GUILayout.Width(125));

            switch (_exportMethod)
            {
                case Enums.ExportImportMethods.CSV:
                    GUILayout.Space(5);

                    _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, _csvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup),
                        GUILayout.Width(45));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save CSV file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save CSV File", "", $"{_language}.csv", "csv");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildCSV();
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                            Close();
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
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save JSON file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save JSON File", "", $"{_language}.json", "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(_jsonSerializer);
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                            Close();
                        }
                    }
                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, "Save XML file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save XML File", "", $"{_language}.xml", "xml");

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(_xmlSerializer);
                            LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                            Close();
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

        private void ShowImportSection()
        {
            GUILayout.BeginVertical(GUILayout.Height(40));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(8);
            _importMethod = (Enums.ExportImportMethods)EditorGUILayout.EnumPopup(_importMethod, CustomStyles.GetStyle(Enums.CustomStyleName.LanguageManageEditorEnumPopup), GUILayout.Width(125));

            switch (_importMethod)
            {
                case Enums.ExportImportMethods.CSV:
                    GUILayout.Space(5);

                    _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, _csvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup),
                        GUILayout.Width(45));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load CSV file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load CSV File", "", "csv");

                        if (!string.IsNullOrEmpty(path))
                        {
                            ImportCSV(path);
                            Close();
                        }
                    }

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load JSON file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load JSON File", "", "json");

                        if (!string.IsNullOrEmpty(path))
                        {
                            ImportSerializedData(path, _jsonSerializer);
                            Close();
                        }
                    }
                    
                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, "Load XML file"), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load XML File", "", "xml");

                        if (!string.IsNullOrEmpty(path))
                        {
                            ImportSerializedData(path, _xmlSerializer);
                            Close();
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUILayout.Space(8);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        #region EXPORTS

        private string BuildCSV()
        {
            var serializer = new CSV_Serializer();
            serializer.SetSeparator(_csvSeparators[_selectedCsvSeparatorIndexForExport]);
            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);

            var titles = new List<string> { "Key", "Category", $"{_language}" };

            serializer.AddLine(titles);

            foreach (var (key, keyData) in data)
            {
                var items = new List<string>();
                var category = keyData.Category;

                items.Add(key);
                items.Add(category);
                items.Add(data[key].LanguagesData[_language].Replace("\n", " ").Replace("\r", " "));

                serializer.AddLine(items);
            }

            return serializer.File();
        }

        private string BuildSerializedData(ISerializerService serializer)
        {
            var dataToSerialize = new LanguageTemplate
            {
                Language = _language
            };

            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);
            foreach (var (key, keyData) in data)
            {
                var category = keyData.Category;
                dataToSerialize.Data.Add(new LanguageTemplate.KeyCategoryLanguage
                {
                    Key = key,
                    Category = category,
                    Value = data[key].LanguagesData[_language]
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
            var headerItems = header.Split(separator);

            if (headerItems.Length < 2)
            {
                EditorUtility.DisplayDialog("Error while importing CSV", $"Separator [ {separator} ] not found", "OK");
                return;
            }

            await LocalizationManager.Instance.ImportLanguage(headerItems[2]);

            var nKeys = 0;
            var nCategories = 0;
            while (!reader.EndOfStream)
            {
                var nextLine = await reader.ReadLineAsync();
                var lineItems = nextLine.Split(separator);
                var key = lineItems[0];
                var category = lineItems[1];
                var value = lineItems[2];

                nKeys++;
                if (!LocalizationManager.Instance.ExistCategory(category)) nCategories++;

                await LocalizationManager.Instance.ImportKey(key, category, _language, value);
            }

            var categories = nCategories > 1 ? "Categories" : "Category";
            sb.AppendLine($"{nCategories} new {categories} imported");
            sb.AppendLine($"{nKeys} keys imported");

            EditorUtility.DisplayDialog("CSV imported", sb.ToString(), "OK");
        }

        private async void ImportSerializedData(string path, ISerializerService serializer)
        {
            var data = await LocalizationManager.LoadFile<LanguageTemplate>(path, serializer);

            var sb = new StringBuilder();
            sb.AppendLine("File has been imported successfully!");

            await LocalizationManager.Instance.ImportLanguage(data.Language);

            var nKeys = 0;
            var nCategories = 0;
            foreach (var keyCategoryLanguage in data.Data)
            {
                nKeys++;
                if (!LocalizationManager.Instance.ExistCategory(keyCategoryLanguage.Category)) nCategories++;

                await LocalizationManager.Instance.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, _language, keyCategoryLanguage.Value);
            }

            var categories = nCategories > 1 ? "Categories" : "Category";
            sb.AppendLine($"{nCategories} new {categories} imported");
            sb.AppendLine($"{nKeys} keys imported");

            EditorUtility.DisplayDialog("File imported", sb.ToString(), "OK");
        }

        #endregion

        #endregion
    }
}
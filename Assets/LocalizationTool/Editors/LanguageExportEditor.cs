using System;
using System.Collections.Generic;
using LocalizationTool.Data;
using LocalizationTool.Data.Json;
using LocalizationTool.Data.Templates;
using LocalizationTool.ExportSerializer;
using LocalizationTool.Manager;
using LocalizationTool.Serializer;
using UnityEditor;
using UnityEngine;


namespace LocalizationTool.Editors
{
    public class LanguageExportEditor : LocalizationEditor
    {
        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(200, 250);

        #endregion

        #region PRIVATE VARIABLES

        private string _language;
        
        private int _selectedCsvSeparatorIndexForExport;

        private ISerializerService _jsonSerializer;
        private ISerializerService _xmlSerializer;

        #endregion

        public static void ShowWindow(string language)
        {
            var window = GetWindow<LanguageExportEditor>("Editor");
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
            ShowHeader($"Export {_language}");
            
            ShowHorizontalLine(5);
            
            GUILayout.Space(5);
            CSV();
            GUILayout.Space(10);
            JSON();
            GUILayout.Space(10);
            XML();

            GUILayout.EndVertical();
        }

        private void CSV()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("CSV", SubHeaderStyle());

            GUILayout.BeginHorizontal();
            GUILayout.Space(75);
            _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, _csvSeparators, SeparatorCSVLanguageWindowStyle());
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), CenterButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save CSV File", "", $"{_language}.csv", "csv");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildCSV();
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                    Close();
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndHorizontal();
        }

        private void JSON()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("JSON", SubHeaderStyle());
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), CenterButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save JSON File", "", $"{_language}.json", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildSerializedData(_jsonSerializer);
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                    Close();
                }
            }
            
            GUILayout.EndHorizontal();
        }

        private void XML()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("XML", SubHeaderStyle());

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs", "Export"), CenterButtonWithIconStyle()))
            {
                var path = EditorUtility.SaveFilePanel("Save XML File", "", $"{_language}.xml", "xml");

                if (!string.IsNullOrEmpty(path))
                {
                    var fileContent = BuildSerializedData(_xmlSerializer);
                    LocalizationManager.SaveFile(path, fileContent, "File Saved", "File has been saved successfully!", "OK");
                    Close();
                }
            }
            
            GUILayout.EndHorizontal();
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

        #endregion
    }
}
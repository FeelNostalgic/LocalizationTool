using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Data.Templates;
using LocalizationTool.Manager;
using LocalizationTool.Serializer;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
    public class LanguageImportEditor : LocalizationEditor
    {
        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(250, 275);

        #endregion

        #region PRIVATE VARIABLES

        private string _language;

        private int _selectedCsvSeparatorIndexForImport;

        private ISerializerService _jsonSerializer;
        private ISerializerService _xmlSerializer;

        #endregion

        public static void ShowWindow(string language)
        {
            var window = GetWindow<LanguageImportEditor>("Editor");
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
            ShowHeader($"Import {_language}");

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
            GUILayout.Label("CSV", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleLeft14Label));

            GUILayout.BeginHorizontal();
            GUILayout.Space(125);
            _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, _csvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup));
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Export"), CustomStyles.GetStyle(Enums.CustomStyleName.CenteredButtonWithIcon)))
            {
                var path = EditorUtility.OpenFilePanel("Load CSV File", "", "csv");

                if (!string.IsNullOrEmpty(path))
                {
                    ImportCSV(path);
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
            GUILayout.Label("JSON", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleLeft14Label));

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Export"), CustomStyles.GetStyle(Enums.CustomStyleName.CenteredButtonWithIcon)))
            {
                //TODO:
                var path = EditorUtility.OpenFilePanel("Load JSON File", "", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    ImportSerializedData(path, _jsonSerializer);
                    Close();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void XML()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("XML", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleLeft14Label));

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon", "Export"), CustomStyles.GetStyle(Enums.CustomStyleName.CenteredButtonWithIcon)))
            {
                //TODO:
                var path = EditorUtility.OpenFilePanel("Load XML File", "", "xml");

                if (!string.IsNullOrEmpty(path))
                {
                    ImportSerializedData(path, _xmlSerializer);
                    Close();
                }
            }

            GUILayout.EndHorizontal();
        }


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
                if(!LocalizationManager.Instance.ExistCategory(category)) nCategories++;

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
                if(!LocalizationManager.Instance.ExistCategory(keyCategoryLanguage.Category)) nCategories++;

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
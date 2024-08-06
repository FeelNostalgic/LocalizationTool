using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LocalizationTool.Data;
using LocalizationTool.Data.Templates;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.ExportSerializer;
using LocalizationTool.Scripts.General;
using LocalizationTool.Scripts.Serializer;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Editors
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
            var window = GetWindow<LanguageManagerEditor>(string.Format(LANGUAGE_MANAGER_WINDOW_TITLE_LABEL, language));
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
            ShowHeader1(string.Format(LANGUAGE_MANAGER_TITLE_LABEL, _language));

            ShowHorizontalLine(5);

            ShowHeader2(EXPORT_LABEL_UPPER);
            ShowExportSection();

            ShowHeader2(IMPORT_LABEL_UPPER);
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

                    _selectedCsvSeparatorIndexForExport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForExport, CsvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup),
                        GUILayout.Width(45));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel(string.Format(EXPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER), "",
                            $"{_language}{CSV_LABEL_UPPER}", CSV_LABEL_LOWER);
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
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel(string.Format(EXPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER), "",
                            $"{_language}{CSV_LABEL_UPPER}", JSON_LABEL_LOWER);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(_jsonSerializer);
                            SaveFile(path, fileContent);
                        }
                    }
                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();

                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ExportIcon, string.Format(EXPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.SaveFilePanel(string.Format(EXPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER), "",
                            $"{_language}{CSV_LABEL_UPPER}", XML_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileContent = BuildSerializedData(_xmlSerializer);
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

        private void SaveFile(string path, string fileContent)
        {
            LocalizationManager.SaveFile(path, fileContent, FILE_SAVED_DIALOG_TITLE, FILE_SAVED_DIALOG_MESSAGE, DIALOG_OK_OPTION);
            Close();
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

                    _selectedCsvSeparatorIndexForImport = EditorGUILayout.Popup(_selectedCsvSeparatorIndexForImport, CsvSeparators, CustomStyles.GetStyle(Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup),
                        GUILayout.Width(45));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, CSV_LABEL_UPPER), "", CSV_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LANGUAGE_LABEL, _language, CSV_LABEL_UPPER));
                            EditorCoroutineUtility.StartCoroutine(ImportCSV(path, progressWindow), progressWindow);
                            
                            Close();
                        }
                    }

                    break;
                case Enums.ExportImportMethods.JSON:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, JSON_LABEL_UPPER), "", JSON_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LANGUAGE_LABEL, _language, JSON_LABEL_UPPER));

                            EditorCoroutineUtility.StartCoroutine(ImportSerializedData(path, _jsonSerializer, progressWindow), progressWindow);
                           
                            Close();
                        }
                    }
                    
                    break;
                case Enums.ExportImportMethods.XML:
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetGUIContent(ImportIcon, string.Format(IMPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER)), CustomStyles.GetStyle(Enums.CustomStyleName.BiggerCenteredButtonWithIcon)))
                    {
                        var path = EditorUtility.OpenFilePanel(string.Format(IMPORT_BUTTON_TOOLTIP, XML_LABEL_UPPER), "", XML_LABEL_LOWER);

                        if (!string.IsNullOrEmpty(path))
                        {
                            var progressWindow = ImportProgressWindow.OpenWindow(string.Format(IMPORT_WINDOW_LANGUAGE_LABEL, _language, XML_LABEL_UPPER));

                            EditorCoroutineUtility.StartCoroutine(ImportSerializedData(path, _xmlSerializer, progressWindow), progressWindow);
                            
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
            serializer.SetSeparator(CsvSeparators[_selectedCsvSeparatorIndexForExport]);
            
            serializer.AddTitle(_language);
            
            var data = new Dictionary<string, KeyData>(LocalizationManager.Dictionary);
            
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

        private IEnumerator ImportCSV(string path, ImportProgressWindow progressWindow)
        {
            using var reader = new StreamReader(path);
            var fileInfo = new FileInfo(path);
            var totalBytes = fileInfo.Length;
            long bytesRead = 0;

            var header = reader.ReadLine();
            Debug.Assert(header != null, nameof(header) + " != null");
            bytesRead += header.Length + Environment.NewLine.Length;
            progressWindow.SetProgress( (float)bytesRead / totalBytes);
            var separator = CsvSeparators[_selectedCsvSeparatorIndexForImport];
            var headerItems = header.Split(separator);

            if (headerItems.Length < 2)
            {
                var sbResultError = new StringBuilder();
                sbResultError.AppendLine("Error while importing CSV");
                sbResultError.AppendLine( $"Separator [ {separator} ] not found");
                progressWindow.Complete(sbResultError.ToString());
                
                yield break;
            }

            var loadLanguageTask = LocalizationManager.Instance.ImportLanguage(headerItems[2]);
            var awaiter = loadLanguageTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;

            var nKeys = 0;
            var nCategories = 0;
            while (!reader.EndOfStream)
            {
                var nextLine = reader.ReadLine();
                System.Diagnostics.Debug.Assert(nextLine != null, nameof(nextLine) + " != null");
                bytesRead += nextLine.Length + Environment.NewLine.Length;
                var lineItems = nextLine.Split(separator);
                var key = lineItems[0];
                var category = lineItems[1];
                var value = lineItems[2];

                nKeys++;
                if (!LocalizationManager.ExistCategory(category)) nCategories++;
                
                var loadKeyTask = LocalizationManager.Instance.ImportKey(key, category, _language, value);
                var awaiterKey = loadKeyTask.GetAwaiter();
                while (!awaiterKey.IsCompleted) yield return null;
                
                progressWindow.SetProgress( (float)bytesRead / totalBytes);
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, key, category));
            }
            
            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            progressWindow.Complete(sb.ToString());
        }

        private IEnumerator ImportSerializedData(string path, ISerializerService serializer, ImportProgressWindow progressWindow)
        {
            var loadFileTask =  LocalizationManager.LoadFile<LanguageTemplate>(path, serializer);
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();
            
            var totalItems = data.Data.Count;
            var itemCount = 0f;
            

            var loadLanguageTask = LocalizationManager.Instance.ImportLanguage(data.Language);
            var awaiterLanguage = loadLanguageTask.GetAwaiter();
            while (!awaiterLanguage.IsCompleted) yield return null;
            
            progressWindow.SetStatus(IMPORT_STATUS_KEYS);
            var nKeys = 0;
            var nCategories = 0;
            foreach (var keyCategoryLanguage in data.Data)
            {
                nKeys++;
                if (!LocalizationManager.ExistCategory(keyCategoryLanguage.Category)) nCategories++;

                var task = LocalizationManager.Instance.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, _language, keyCategoryLanguage.Value);
                var awaiterKey = task.GetAwaiter();
                while (!awaiterKey.IsCompleted) yield return null;
                
                progressWindow.SetProgress(itemCount++ / totalItems);
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, keyCategoryLanguage.Key, keyCategoryLanguage.Category));
            }
            
            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            progressWindow.Complete(sb.ToString());
        }

        #endregion

        #endregion
    }
}
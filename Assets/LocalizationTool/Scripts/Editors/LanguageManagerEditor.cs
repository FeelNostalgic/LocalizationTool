#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.Data.TemplatesForSerializer;
using LocalizationTool.Scripts.ExportSerializer;
using LocalizationTool.Scripts.General;
using LocalizationTool.Scripts.Serializer;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Editors.EditorWindowAbstract;

namespace LocalizationTool.Scripts.Editors
{
    public class LanguageManagerEditor : EditorWindow
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

        private void OnGUI()
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
                            $"{_language}{JSON_LABEL_UPPER}", JSON_LABEL_LOWER);
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
                            $"{_language}{XML_LABEL_UPPER}", XML_LABEL_LOWER);

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
            SaveLoadFileManager.SaveFile(path, fileContent, FILE_SAVED_DIALOG_TITLE, FILE_SAVED_DIALOG_MESSAGE, DIALOG_OK_OPTION);
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
            var serializerCsv = new CSV_Serializer();
            serializerCsv.SetSeparator(CsvSeparators[_selectedCsvSeparatorIndexForExport]);

            serializerCsv.AddTitle(_language);
            
            foreach (var keyData in CacheDataSO.Keys)
            {
                var line = new List<string>();
                var key = keyData.keyName;
                var category = keyData.CategoryName;
                
                line.Add(key);
                line.Add(category);
                var text = CacheDataSO.localizationData.LanguagesDictionary[_language].TranslationDictionary[key].translationText;
                line.Add(text.Replace("\n", " ").Replace("\r", " "));
                
                serializerCsv.AddLine(line);
            }
            
            return serializerCsv.File();
        }

        private string BuildSerializedData(ISerializerService serializer)
        {
            var dataToSerialize = new LanguageTemplate
            {
                Language = _language
            };
            
            foreach (var keyData in CacheDataSO.Keys)
            {
                var key = keyData.keyName;
                var category = keyData.CategoryName;
                
                dataToSerialize.Data.Add(new LanguageTemplate.KeyCategoryLanguage
                {
                    Key = key,
                    Category = category,
                    Value = CacheDataSO.localizationData.LanguagesDictionary[_language].TranslationDictionary[key].translationText
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
            progressWindow.SetProgress((float)bytesRead / totalBytes);
            var separator = CsvSeparators[_selectedCsvSeparatorIndexForImport];
            var headerItems = header.Split(separator);

            if (headerItems.Length < 2)
            {
                var sbResultError = new StringBuilder();
                sbResultError.AppendLine("Error while importing CSV");
                sbResultError.AppendLine($"Separator [ {separator} ] not found");
                progressWindow.Complete(sbResultError.ToString());

                yield break;
            }

            LocalizationManager.ImportLanguage(headerItems[2]);

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
                if (!LocalizationManager.ExistsCategory(category)) nCategories++;

                LocalizationManager.ImportKey(key, category, _language, value);

                progressWindow.SetProgress((float)bytesRead / totalBytes);
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
            var loadFileTask = SaveLoadFileManager.LoadFile<LanguageTemplate>(path, serializer);
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            var totalItems = data.Data.Count;
            var itemCount = 0f;


            LocalizationManager.ImportLanguage(data.Language);

            progressWindow.SetStatus(IMPORT_STATUS_KEYS);
            var nKeys = 0;
            var nCategories = 0;
            foreach (var keyCategoryLanguage in data.Data)
            {
                nKeys++;
                if (!LocalizationManager.ExistsCategory(keyCategoryLanguage.Category)) nCategories++;

                LocalizationManager.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, _language, keyCategoryLanguage.Value);

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
#endif
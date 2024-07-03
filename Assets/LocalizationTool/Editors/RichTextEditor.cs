using System;
using System.Reflection;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editor
{
    public class RichTextEditor : LocalizationEditor
    {
        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(500, 600);

        #endregion

        #region PRIVATE VARIABLES

        private string _richText = "";
        private string _key;
        private bool _showRichTextTags = true;
        private int _fontSizeIndex = 5;
        private string[] _fontSizes = { "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "24", "26", "28", "30", "32" };
        private int _currentFontSizeIndex = 5;
        
        private Action<string> _onTextChanged;

        #endregion

        public static void ShowWindow(string initialText, string key, Action<string> onTextChanged)
        {
            var window = GetWindow<RichTextEditor>("Editor");
            window.minSize = WindowSize;
            window._key = key;
            window._richText = initialText;
            window._onTextChanged = onTextChanged;
            window._currentFontSizeIndex = window._fontSizeIndex;
        }

        private void OnDestroy()
        {
            _onTextChanged?.Invoke(_richText);
        }

        #region PRIVATE METHODS

        private void OnGUI()
        {
            GUILayout.BeginVertical(MaxHeightOption(_windowSize.y), MaxWidthOption(_windowSize.x));

            GUILayout.Space(5);

            EditorGUILayout.SelectableLabel(_key, KeyLabelStyle(), MaxHeightOption(24));

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            StyleButtons();

            FontSizeButtons();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            _showRichTextTags = EditorGUILayout.Toggle("Show Rich Text", _showRichTextTags);

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("TextArea");
            var richTextStyle = new GUIStyle(GUI.skin.textArea)
            {
                richText = _showRichTextTags
            };

            _richText = EditorGUILayout.TextArea(_richText, richTextStyle, GUILayout.Height(450));
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Apply", ButtonStyle(), MinHeightOption(30)))
            {
                _onTextChanged?.Invoke(_richText);
                Close();
            }

            GUILayout.EndVertical();
        }

        private void FontSizeButtons()
        {
            //TODO
            GUILayout.BeginHorizontal();

            GUILayout.Label("Font Size", GUILayout.Width(70));
            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            _fontSizeIndex = EditorGUILayout.Popup(_fontSizeIndex, _fontSizes, GUILayout.Width(70));
            if (_currentFontSizeIndex != _fontSizeIndex)
            {
                _currentFontSizeIndex = _fontSizeIndex;
                ApplyStyle(Enums.RICH_TEXT_STYLE.FontSize);
            }

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                if (_fontSizeIndex < _fontSizes.Length - 1)
                {
                    _fontSizeIndex++;
                    ApplyStyle(Enums.RICH_TEXT_STYLE.FontSize);
                }
            }

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                if (_fontSizeIndex > 0)
                {
                    _fontSizeIndex--;
                    ApplyStyle(Enums.RICH_TEXT_STYLE.FontSize);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void StyleButtons()
        {
            if (GUILayout.Button("B", GUILayout.Width(30)))
            {
                ApplyStyle(Enums.RICH_TEXT_STYLE.Bold);
            }

            if (GUILayout.Button("I", GUILayout.Width(30)))
            {
                ApplyStyle(Enums.RICH_TEXT_STYLE.Italic);
            }

            //TODO: Fixed this
            // if (GUILayout.Button("U", GUILayout.Width(30)))
            // {
            //     ApplyStyle("<u>", "</u>");
            // }
        }

        private void ApplyStyle(Enums.RICH_TEXT_STYLE style)
        {
            // Get the current TextEditor
            var tEditor = typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as TextEditor;

            if (tEditor == null) return;

            var startIndex = Mathf.Min(tEditor.cursorIndex, tEditor.selectIndex);
            var length = Mathf.Abs(tEditor.cursorIndex - tEditor.selectIndex);

            if (length == 0) return;

            var selectedText = _richText.Substring(startIndex, length);
            
            bool removeOrAdd;
            string styledText;
            
            if (style == Enums.RICH_TEXT_STYLE.FontSize)
            {
                removeOrAdd = !selectedText.Contains("size");
                if (removeOrAdd)
                {
                    //Add Tags
                    var (openingTag, closingTag) = GetTags(style);
                    styledText = $"{openingTag}{selectedText}{closingTag}";
                    _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
                }
                else
                {
                    //Remove Tags
                    var numberStartIndex = selectedText.IndexOf("<size=", StringComparison.InvariantCulture) + 6;
                    var numberEndIndex = 0;
                    for (var i = numberStartIndex; i < selectedText.Length; i++)
                    {
                        if (selectedText[i] != '>') continue;
                        numberEndIndex = i;
                        break;
                    }
                    var (openingTag, closingTag) = GetTags(style);
                    var oldOpeningTag = $"<size={selectedText.Substring(numberStartIndex, numberEndIndex - numberStartIndex)}>";

                    var textWithoutTags = selectedText.Replace(oldOpeningTag, "");
                    textWithoutTags = textWithoutTags.Replace(closingTag, "");
                    
                    //Add new font size
                    styledText = $"{openingTag}{textWithoutTags}{closingTag}";
                    _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
                }
            }
            else
            {
                var (openingTag, closingTag) = GetTags(style);
                removeOrAdd = !selectedText.Contains(openingTag);
                if (removeOrAdd)
                {
                    //Add Tags
                    styledText = $"{openingTag}{selectedText}{closingTag}";
                    _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
                }
                else
                {
                    //Remove Tags
                    styledText = selectedText.Replace(openingTag, "");
                    styledText = styledText.Replace(closingTag, "");

                    _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
                }
            }



            // Move cursor to the end of the newly styled text
            tEditor.cursorIndex = startIndex + styledText.Length;
            tEditor.selectIndex = startIndex + styledText.Length;

            GUI.FocusControl(null);
        }

        private (string, string) GetTags(Enums.RICH_TEXT_STYLE style)
        {
            return style switch
            {
                Enums.RICH_TEXT_STYLE.Bold => ("<b>", "</b>"),
                Enums.RICH_TEXT_STYLE.Italic => ("<i>", "</i>"),
                Enums.RICH_TEXT_STYLE.FontSize => ($"<size={_fontSizes[_fontSizeIndex]}>", "</size>"),
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
            };
        }

        #endregion
    }
}
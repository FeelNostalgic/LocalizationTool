using System;
using System.Reflection;
using LocalizationTool.Commons;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    public class RichTextEditor : LocalizationEditor
    {
        #region DIMENSION VARIABLES

        private static readonly Vector2 WindowSize = new(700, 650);

        #endregion

        #region PRIVATE VARIABLES

        private const int FONT_SIZE_TEXT_AREA = 12;
        
        // If you want more font size, just add them to this array
        private readonly string[] _fontSizes = { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "24", "26", "28", "30", "32", "36", "40", "42", "44", "48", "50", "54", "58", "62", "68", "72", "100", "110" };

        private string _richText = "";
        private string _key;
        private bool _showRichTextTags = true;
        private int _fontSizeIndex = 2;
        private int _currentZoom = 100;
        private Vector2 _scrollView;
        private Color _colorSelected = Color.white;
        
        #endregion

        public static void ShowWindow(string initialText, string key, Action<string> onTextChanged)
        {
            var window = GetWindow<RichTextEditor>("Editor");
            window.minSize = WindowSize;
            window.maxSize = new Vector2(WindowSize.x, 100000);
            window._key = key;
            window._richText = initialText;
        }
        
        #region PRIVATE METHODS

        private new void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);

            //TODO: poder editar key
            EditorGUILayout.SelectableLabel(_key, CustomStyles.GetStyle(Enums.CustomStyleName.KeyFixedHeightSelectableLabel));

            EditorGUILayout.BeginHorizontal("box", GUILayout.Height(35));

            GUILayout.Space(3);

            HistoryButtons();

            Separator();

            StyleButtons();

            Separator();

            FontSizeButtons();

            Separator();

            ColorButtons();

            Separator();

            ZoomButtons();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            TextArea();

            RichTextToggle();

            CloseButton();

            GUILayout.EndVertical();
        }

        private void TextArea()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            GUI.SetNextControlName("TextArea");
            var richTextStyle = new GUIStyle(GUI.skin.textArea)
            {
                richText = _showRichTextTags,
                wordWrap = true,
                fontSize = FONT_SIZE_TEXT_AREA * _currentZoom / 100,
                padding = { top = 5, bottom = 5, left = 5, right = 5 },
                normal =
                {
                    textColor = Color.white
                }
            };

            GUILayout.BeginHorizontal("box");
            _scrollView = GUILayout.BeginScrollView(_scrollView, GUILayout.ExpandHeight(true));

            var temp = EditorGUILayout.TextArea(_richText, richTextStyle, GUILayout.ExpandHeight(true));
            UpdateValue(_key, temp); 

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
        }

        private void RichTextToggle()
        {
            EditorGUILayout.BeginHorizontal("box", GUILayout.Height(30));
            _showRichTextTags = ToggleLeft(_showRichTextTags, "Enable rich text preview but some of the effects are only appreciate with TextMesh PRO");
            EditorGUILayout.EndHorizontal();
        }

        private void CloseButton()
        {
            GUILayout.BeginVertical(GUILayout.Height(35));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Colors.DEFAULT;

            
            if (GUILayout.Button("Close", CustomStyles.GetStyle(Enums.CustomStyleName.CloseRichTextEditorButton))) { Close(); }

            GUILayout.FlexibleSpace();

            var context = GetGUIContent(ChangesSavedIcon, "Changes are stored automatically");
            GUILayout.Label(context, GUILayout.Height(30), GUILayout.Width(25));

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private static bool ToggleLeft(bool value, string label)
        {
            GUILayout.BeginHorizontal();
            var temp = EditorGUILayout.Toggle(value, GUILayout.Height(28), GUILayout.Width(15));
            EditorGUILayout.LabelField(label, CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationToggleLabel), GUILayout.Height(28));
            GUILayout.EndHorizontal();

            return temp;
        }

        private void HistoryButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(GetGUIContent(UndoIcon, "Undo action"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                //TODO
            }

            if (GUILayout.Button(GetGUIContent(RedoIcon, "Redo action"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                //TODO
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void StyleButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("B", "Apply bold selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Bold);
            }

            if (GUILayout.Button(new GUIContent("I", "Apply italic to selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Italic);
            }

            //Fixed this: not work with textArea
            if (GUILayout.Button(new GUIContent("U", "Apply underline to selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Underline);
            }

            if (GUILayout.Button(new GUIContent("S", "Apply strike out to selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.StrikeOut);
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void FontSizeButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _fontSizeIndex = EditorGUILayout.Popup(_fontSizeIndex, _fontSizes, CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextFontSizePopup), GUILayout.Width(60));

            if (GUILayout.Button(GetGUIContent(ApplyStyleIcon, "Apply font size to selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.FontSize);
            }
            
            if (GUILayout.Button(GetGUIContent(AddEmptyIcon, "Do font bigger"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                if (_fontSizeIndex < _fontSizes.Length - 1)
                {
                    _fontSizeIndex++;
                    ApplyStyle(Enums.RichTextStyle.FontSize);
                }
            }

            if (GUILayout.Button(GetGUIContent(MinusEmptyIcon, "Do font smaller"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                if (_fontSizeIndex > 0)
                {
                    _fontSizeIndex--;
                    ApplyStyle(Enums.RichTextStyle.FontSize);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ColorButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            _colorSelected = EditorGUILayout.ColorField(GUIContent.none, _colorSelected, false, true, false, GUILayout.Height(28), GUILayout.Width(40));
            if (GUILayout.Button(GetGUIContent(ApplyStyleIcon, "Apply color to selected text"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Color);
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ZoomButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{_currentZoom} %", CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextZoomLabel), GUILayout.Width(40));

            if (GUILayout.Button(GetGUIContent(AddEmptyIcon, "Zoom In"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                _currentZoom += 10;
                _currentZoom = Math.Clamp(_currentZoom, 60, 250);
            }

            if (GUILayout.Button(GetGUIContent(MinusEmptyIcon, "Zoom Out"), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                _currentZoom -= 10;
                _currentZoom = Math.Clamp(_currentZoom, 60, 250);
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private static void Separator()
        {
            GUILayout.BeginVertical(GUILayout.Width(12));
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(" | ", GUILayout.Width(12), GUILayout.Height(30));

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void ApplyStyle(Enums.RichTextStyle style)
        {
            // Get the current TextEditor

            // You have to have focus on textArea for this to work
            if (typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) is not TextEditor tEditor) return;

            var startIndex = Mathf.Min(tEditor.cursorIndex, tEditor.selectIndex);
            var length = Mathf.Abs(tEditor.cursorIndex - tEditor.selectIndex);

            if (length == 0)
            {
                LocalizationManager.LogWarning("Select some text to apply a style");
                return;
            }

            var selectedText = _richText.Substring(startIndex, length);

            switch (style)
            {
                case Enums.RichTextStyle.Bold or Enums.RichTextStyle.Italic or Enums.RichTextStyle.Underline or Enums.RichTextStyle.StrikeOut:
                    ApplyBasicStyle(style, selectedText, startIndex, length);
                    break;
                case Enums.RichTextStyle.Color:
                    ApplyColorStyle(style, selectedText, startIndex, length);
                    break;
                case Enums.RichTextStyle.FontSize:
                    ApplyFontSize(style, selectedText, startIndex, length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }

            Repaint();

            // Move cursor to the end of the newly styled text
            //tEditor.cursorIndex = startIndex + (styledText.Length - openingTag.Length - closingTag.Length);
            //tEditor.selectIndex = startIndex + (styledText.Length - openingTag.Length - closingTag.Length);

            GUI.FocusControl(null);
        }

        private void ApplyBasicStyle(Enums.RichTextStyle style, string selectedText, int startIndex, int length)
        {
            string styledText, openingTag, closingTag;

            (openingTag, closingTag) = GetTags(style);

            var removeOrAdd = !selectedText.Contains(openingTag);

            // If contains openingTag you have to remove it
            if (removeOrAdd)
            {
                AddTags(selectedText, startIndex, length, openingTag, closingTag);
            }
            else
            {
                //Remove Tags
                styledText = selectedText.Replace(openingTag, "");
                styledText = styledText.Replace(closingTag, "");

                _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
            }
        }

        private void ApplyColorStyle(Enums.RichTextStyle style, string selectedText, int startIndex, int length)
        {
            var removeOrAdd = !selectedText.Contains("color");
            string styledText, openingTag, closingTag;
            (openingTag, closingTag) = GetTags(style);

            if (removeOrAdd)
            {
                AddTags(selectedText, startIndex, length, openingTag, closingTag);
            }
            else
            {
                //Remove Tags
                var numberStartIndex = selectedText.IndexOf("<color=", StringComparison.InvariantCulture);

                var oldOpeningTag = selectedText.Substring(numberStartIndex, numberStartIndex + 17);

                var textWithoutTags = selectedText.Replace(oldOpeningTag, "");
                textWithoutTags = textWithoutTags.Replace(closingTag, "");

                //Add new color
                styledText = $"{openingTag}{textWithoutTags}{closingTag}";
                _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
            }
        }

        private void ApplyFontSize(Enums.RichTextStyle style, string selectedText, int startIndex, int length)
        {
            var removeOrAdd = !selectedText.Contains("size");
            string styledText, openingTag, closingTag;
            (openingTag, closingTag) = GetTags(style);

            if (removeOrAdd)
            {
                AddTags(selectedText, startIndex, length, openingTag, closingTag);
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

                var oldOpeningTag = $"<size={selectedText.Substring(numberStartIndex, numberEndIndex - numberStartIndex)}>";

                var textWithoutTags = selectedText.Replace(oldOpeningTag, "");
                textWithoutTags = textWithoutTags.Replace(closingTag, "");

                //Add new font size
                styledText = $"{openingTag}{textWithoutTags}{closingTag}";
                _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
            }
        }

        private void AddTags(string selectedText, int startIndex, int length, string openingTag, string closingTag)
        {
            string styledText;
            //Add Tags
            styledText = $"{openingTag}{selectedText}{closingTag}";
            _richText = $"{_richText[..startIndex]}{styledText}{_richText[(startIndex + length)..]}";
        }

        private (string, string) GetTags(Enums.RichTextStyle style)
        {
            return style switch
            {
                Enums.RichTextStyle.Bold => ("<b>", "</b>"),
                Enums.RichTextStyle.Italic => ("<i>", "</i>"),
                Enums.RichTextStyle.FontSize => ($"<size={_fontSizes[_fontSizeIndex]}>", "</size>"),
                Enums.RichTextStyle.Underline => ("<u>", "</u>"),
                Enums.RichTextStyle.StrikeOut => ("<s>", "</s>"),
                Enums.RichTextStyle.Color => ($"<color=#{ColorUtility.ToHtmlStringRGBA(_colorSelected)}>", "</color>"),
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
            };
        }

        private async void UpdateValue(string key, string tempValue)
        {
            _richText = tempValue;
            await LocalizationManager.Instance.ChangeValue(key, tempValue);
        }

        #endregion
    }
#endif
}
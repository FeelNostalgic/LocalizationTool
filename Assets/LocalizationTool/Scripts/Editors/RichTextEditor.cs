#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Editors.EditorWindowAbstract;

namespace LocalizationTool.Scripts.Editors
{
    public class RichTextEditor : EditorWindow
    {
        #region PUBLIC VARIABLES
        
        public bool AddActionRunning { get; set; }

        #endregion
        
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
        private int _zoom = 100;
        private Vector2 _scrollView;
        private Color _colorSelected = Color.white;

        private string _tempValue;
        private EditorCoroutine _currentCoroutine;

        #endregion

        public static void ShowWindow(string initialText, string key, Action<string> onTextChanged)
        {
            var window = GetWindow<RichTextEditor>(RICH_TEXT_EDITOR_WINDOW_LABEL);
            window.minSize = WindowSize;
            window.maxSize = new Vector2(WindowSize.x, 100000);
            window._key = key;
            window._richText = initialText;
        }
        
        public void ControlFeedbackLabel(string text)
        {
            AddActionRunning = true;
            if (_currentCoroutine != null) EditorCoroutineUtility.StopCoroutine(_currentCoroutine);
            _currentCoroutine = EditorCoroutineUtility.StartCoroutine(FeedbackLabelCoroutine(text, delegate(string s) { FeedbackLabel = s; }, 1.6f), this);
        }
        
        private IEnumerator FeedbackLabelCoroutine(string text, Action<string> updateFeedbackLabel, float duration, Action onComplete = null)
        {
            updateFeedbackLabel?.Invoke(text);
            yield return new WaitForSecondsRealtime(duration);
            updateFeedbackLabel?.Invoke("");
            LocalizationMainEditor.Instance.Repaint();
            _currentCoroutine = null;
            AddActionRunning = false;

            onComplete?.Invoke();
        }

        protected void OnDestroy()
        {
            UpdateValue(_key, _tempValue);
        }

        #region PRIVATE METHODS

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(5);
            
            KeyTextField();

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

        private void KeyTextField()
        {
            GUILayout.BeginVertical("box",GUILayout.Height(FeedbackLabel.IsNotEmpty() ? 60 : 35));

            GUI.SetNextControlName("VALUE");
            var temp = EditorGUILayout.TextField(_key, CustomStyles.GetStyle(Enums.CustomStyleName.RichTextEditorKeyTextField), GUILayout.Height(30));
            UpdateKey(_key, temp);

            if (FeedbackLabel.IsNotEmpty())
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.FlexibleSpace();
                GUILayout.Label(WarningIcon, GUILayout.Width(20));
                GUILayout.Label(FeedbackLabel, CustomStyles.GetStyle(Enums.CustomStyleName.RichTextEditorFeedbackLabel));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
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
                fontSize = FONT_SIZE_TEXT_AREA * _zoom / 100,
                padding = { top = 5, bottom = 5, left = 5, right = 5 },
                normal =
                {
                    textColor = Color.white
                }
            };

            GUILayout.BeginHorizontal("box");
            _scrollView = GUILayout.BeginScrollView(_scrollView, GUILayout.ExpandHeight(true));
            
            _tempValue = EditorGUILayout.TextArea(_richText, richTextStyle, GUILayout.ExpandHeight(true));
            UpdateValue(_key, _tempValue);

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
        }

        private void RichTextToggle()
        {
            EditorGUILayout.BeginHorizontal("box", GUILayout.Height(30));
            _showRichTextTags = ToggleLeft(_showRichTextTags, RICH_TEXT_EDITOR_TOGGLE_LABEL);
            EditorGUILayout.EndHorizontal();
        }
        
        private static bool ToggleLeft(bool value, string label)
        {
            GUILayout.BeginHorizontal();
            var temp = EditorGUILayout.Toggle(value, GUILayout.Height(28), GUILayout.Width(15));
            EditorGUILayout.LabelField(label, CustomStyles.GetStyle(Enums.CustomStyleName.ConfigurationToggleLabel), GUILayout.Height(28));
            GUILayout.EndHorizontal();

            return temp;
        }

        private void CloseButton()
        {
            GUILayout.BeginVertical(GUILayout.Height(35));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.clear;
            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Colors.DEFAULT;
            
            if (GUILayout.Button(CLOSE_BUTTON_LABEL, CustomStyles.GetStyle(Enums.CustomStyleName.CloseRichTextEditorButton))) Close(); 

            GUILayout.FlexibleSpace();

            var context = GetGUIContent(ChangesSavedIcon, RICH_TEXT_EDITOR_AUTOSAVED_TOOLTIP);
            GUILayout.Label(context, GUILayout.Height(30), GUILayout.Width(25));

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
        
        //FUTURE: HistoryButtons
        private void HistoryButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(GetGUIContent(UndoIcon, RICH_TEXT_EDITOR_UNDO_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                //FUTURE: Undo
            }

            if (GUILayout.Button(GetGUIContent(RedoIcon, RICH_TEXT_EDITOR_REDO_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                //FUTURE: Redo
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

            if (GUILayout.Button(new GUIContent("B", RICH_TEXT_EDITOR_BOLD_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Bold);
            }

            if (GUILayout.Button(new GUIContent("I", RICH_TEXT_EDITOR_ITALIC_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Italic);
            }

            //Fixed this: not work with textArea
            if (GUILayout.Button(new GUIContent("U", RICH_TEXT_EDITOR_UNDERLINE_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.Underline);
            }

            if (GUILayout.Button(new GUIContent("S", RICH_TEXT_EDITOR_STRIKE_OUT_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
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

            if (GUILayout.Button(GetGUIContent(ApplyStyleIcon, RICH_TEXT_EDITOR_FONT_SIZE_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                ApplyStyle(Enums.RichTextStyle.FontSize);
            }
            
            if (GUILayout.Button(GetGUIContent(AddEmptyIcon, RICH_TEXT_EDITOR_BIGGER_FONT_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                if (_fontSizeIndex < _fontSizes.Length - 1)
                {
                    _fontSizeIndex++;
                    ApplyStyle(Enums.RichTextStyle.FontSize);
                }
            }

            if (GUILayout.Button(GetGUIContent(MinusEmptyIcon, RICH_TEXT_EDITOR_SMALLER_FONT_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
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

            _colorSelected = EditorGUILayout.ColorField(new GUIContent("", RICH_TEXT_EDITOR_SELECTED_COLOR_TOOLTIP),
                _colorSelected, false, true, false, GUILayout.Height(28), GUILayout.Width(40));
            if (GUILayout.Button(GetGUIContent(ApplyStyleIcon, RICH_TEXT_EDITOR_COLOR_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
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

            EditorGUILayout.LabelField(string.Format(RICH_TEXT_ZOOM_LABEL, _zoom), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextZoomLabel), GUILayout.Width(40));

            if (GUILayout.Button(GetGUIContent(AddEmptyIcon, RICH_TEXT_EDITOR_ZOOM_IN_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                _zoom += 10;
                _zoom = Math.Clamp(_zoom, 60, 250);
            }

            if (GUILayout.Button(GetGUIContent(MinusEmptyIcon, RICH_TEXT_EDITOR_ZOOM_OUT_BUTTON_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.OptionRichTextButton)))
            {
                _zoom -= 10;
                _zoom = Math.Clamp(_zoom, 60, 250);
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
                LocalizationManager.LogWarning("Rich Text Editor", CustomDebugPlugin.Colors.LightBlue, RICH_TEXT_EDITOR_SELECT_TEXT_WARNING_LOG);
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

        private void UpdateValue(string key, string tempValue)
        {
            _richText = tempValue;
            LocalizationManager.ChangeValue(key, tempValue);
        }

        private void UpdateKey(string oldKey, string newKey)
        {
            _key = LocalizationManager.ChangeKey(oldKey, newKey, this);
        }

        #endregion
    }
}
#endif

using System.Collections.Generic;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.GameUtils;

namespace LocalizationTool.Scripts.Commons
{
    public static class CustomStyles
    {
        private static Dictionary<Enums.CustomStyleName, GUIStyle> _styles;

        public static GUIStyle GetStyle(Enums.CustomStyleName name)
        {
            if (_styles.IsNull()) LoadDictionary();
            return _styles[name];
        }

        private static void LoadDictionary()
        {
            _styles = new Dictionary<Enums.CustomStyleName, GUIStyle>
            {
                {
                    Enums.CustomStyleName.CenteredButtonWithIcon, new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 24,
                        fixedWidth = 28
                    }
                },
                {
                    Enums.CustomStyleName.BiggerCenteredButtonWithIcon, new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 28,
                        fixedWidth = 28
                    }
                },
                {
                    Enums.CustomStyleName.ConfigurationExportImportButton, new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 32,
                        fixedWidth = 32
                    }
                },
                {
                    Enums.CustomStyleName.KeyTextField, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.SeparatorsCsvPopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 32,
                        fontSize = 18,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.SeparatorsCsvLanguageWindowPopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 30,
                        fixedWidth = 0,
                        fontSize = 15,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ScrollViewCategoryPopup, new GUIStyle(EditorStyles.popup)
                    {
                        fixedHeight = 30,
                        fixedWidth = 0
                    }
                },
                {
                    Enums.CustomStyleName.CategoryPopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 30,
                        fixedWidth = 0,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.SearchTypePopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 32,
                        fixedWidth = 150,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.FeedbackLabel, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 12,
                        wordWrap = true,
                        richText = true,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.CloseRichTextEditorButton, new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 13,
                        fixedHeight = 30,
                        fixedWidth = 150,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.Header1BoldMiddleCenter20Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 20,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.Header2BoldMiddleCenter15Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 15,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.Header2BoldMiddleLeft15Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = FontStyle.Bold,
                        fontSize = 15,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.Header2LowerCenter14Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerCenter,
                        fontSize = 14,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.Header1BoldMiddleCenter15Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 15,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.KeySelectableLabel, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.KeyFixedHeightSelectableLabel, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        fixedHeight = 24,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ValueMiddleLeftTextField, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        richText = true,
                        wordWrap = true,
                        fontSize = 14,
                        border = { left = 0, right = 0, top = 0, bottom = 0 },
                        margin = { left = 0, right = 0, top = 0, bottom = 0 },
                        padding = { left = 15, right = 0, top = 0, bottom = 0 },
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.OrderIntField, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 13,
                        normal =
                        {
                            textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ValueEditorPreviewTextArea, new GUIStyle(GUI.skin.textArea)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        richText = true,
                        fixedHeight = 0,
                        fontSize = 12,
                        border = { left = 0, right = 0, top = 0, bottom = 0 },
                        margin = { left = 0, right = 0, top = 0, bottom = 0 },
                        padding = { left = 3, right = 3, top = 2, bottom = 1 },
                        normal =
                        {
                            background = GetTexture2DFromColor(Colors.Alpha(Color.black, 0)),
                            textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                        },
                        active = { background = GetTexture2DFromColor(Colors.Alpha(Color.black, 0)) },
                        focused = { background = GetTexture2DFromColor(Colors.Alpha(Color.black, 0)) }
                    }
                },
                {
                    Enums.CustomStyleName.ValueEditorPreviewBox, new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        border = { left = 0, right = 0, top = 0, bottom = 0 },
                        margin = { left = 0, right = 0, top = 0, bottom = 0 },
                        padding = { left = 1, right = 1, top = 1, bottom = 1 },
                        normal =
                        {
                            background = GetTexture2DFromColor(EditorGUIUtility.isProSkin ? Colors.Alpha(Color.black, .1f) : Colors.Alpha(Color.white, .5f)),
                            textColor = EditorGUIUtility.isProSkin ? Colors.Alpha(Color.white) : Colors.Alpha(Color.black, .7f)
                        },
                        hover = { textColor = EditorGUIUtility.isProSkin ? Colors.Alpha(Color.white) : Colors.Alpha(Color.black, .7f) }
                    }
                },
                {
                    Enums.CustomStyleName.LanguageManageEditorEnumPopup, new GUIStyle(EditorStyles.popup)
                    {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 30,
                        fixedWidth = 0,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ConfigurationExportImportEnumPopup, new GUIStyle(EditorStyles.popup)
                    {
                        fontSize = 15,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 32,
                        fixedWidth = 0,
                        padding = {top = 0, bottom = 0, left = 10, right = 0},
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ConfigurationToggleLabel, new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.ConfigurationReadmeButton, new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        fixedHeight = 32,
                        fixedWidth = 150,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.OptionRichTextButton, new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 12,
                        fixedHeight = 28,
                        fixedWidth = 28,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.OptionRichTextFontSizePopup, new GUIStyle(EditorStyles.popup)
                    {
                        fontSize = 12,
                        fixedHeight = 28,
                        fixedWidth = 0,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.OptionRichTextZoomLabel, new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 12,
                        fixedHeight = 28,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.AddonSelectableTextFieldLabel, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.UpperLeft,
                        richText = true,
                        wordWrap = true,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.RichTextEditorKeyTextField, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.RichTextEditorFeedbackLabel, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        wordWrap = true,
                        richText = true,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                }
            };
        }
    }
}
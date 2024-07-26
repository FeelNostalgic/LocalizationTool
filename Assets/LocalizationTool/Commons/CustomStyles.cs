using System.Collections.Generic;
using LocalizationTool.Data;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Commons.GameUtils;

namespace LocalizationTool.Commons
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
                    Enums.CustomStyleName.ConfigurationExportImportButton, new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 30,
                        fixedWidth = 30
                    }
                },
                {
                    Enums.CustomStyleName.KeyTextField, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        fixedHeight = 25,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.SeparatorsCsvPopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.UpperCenter,
                        fixedHeight = 28,
                        fixedWidth = 55,
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
                        fixedHeight = 22,
                        fixedWidth = 45,
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
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 25,
                        fixedWidth = 250,
                        fontSize = 12,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                },
                {
                    Enums.CustomStyleName.CategoryPopup, new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 24,
                        fontSize = 12,
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
                        fixedHeight = 24,
                        fixedWidth = 150,
                        fontSize = 12,
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
                    Enums.CustomStyleName.Header2BoldMiddleCenter14Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 14,
                        normal =
                        {
                            textColor = Color.white
                        }
                    }  
                },
                {
                    Enums.CustomStyleName.Header2BoldMiddleLeft14Label, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = FontStyle.Bold,
                        fontSize = 14,
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
                    Enums.CustomStyleName.KeyFixedHeightFixedWidthSelectableLabel, new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        fixedHeight = 24,
                        fixedWidth = 300,
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
                        fontSize = 13,
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
                        fontSize = 13,
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
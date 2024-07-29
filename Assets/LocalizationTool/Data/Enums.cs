using System;
using System.Collections.Generic;

namespace LocalizationTool.Data
{
    public class Enums
    {
        public enum GUIWindow
        {
            Dictionary = 0,
            Languages = 1,
            Categories = 2,
            Configuration = 3
        }

        public enum RichTextStyle
        {
            Bold = 0,
            Italic = 1,
            FontSize = 2
        }

        public enum ExportImportMethods
        {
            CSV,
            JSON,
            XML
        }
        
        public enum CustomStyleName
        {
            CenteredButtonWithIcon,
            BiggerCenteredButtonWithIcon,
            ConfigurationExportImportButton,
            KeyTextField,
            SeparatorsCsvPopup,
            SeparatorsCsvLanguageWindowPopup,
            ScrollViewCategoryPopup,
            CategoryPopup,
            SearchTypePopup,
            FeedbackLabel,
            CloseRichTextEditorButton,
            Header1BoldMiddleCenter20Label,
            Header2BoldMiddleCenter15Label,
            Header2BoldMiddleLeft14Label,
            Header1BoldMiddleCenter15Label,
            Header2LowerCenter14Label,
            KeySelectableLabel,
            KeyFixedHeightSelectableLabel,
            ValueMiddleLeftTextField,
            OrderIntField,
            ColumnsTitleBoldMiddleLeftLabel,
            ValueEditorPreviewTextArea,
            ValueEditorPreviewBox,
            LanguageManageEditorEnumPopup
        }
        
        public static string[] SEARCH_TYPE = {"By key", "By value", "By both"};
    }
    
    [Serializable]
    public struct KeyData
    {
        public string Category;
        public Dictionary<string, string> LanguagesData;
    }
}
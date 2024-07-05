using System;
using LocalizationTool.Data;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{ 
	public class ConfigurationEditor : LocalizationEditor
	{
        #region PUBLIC VARIABLES
        
        #endregion

        #region PRIVATE VARIABLES

        #region EDITOR VARIABLES

        private bool _toggleAllDeleteConfirmation;
        private bool _dictionaryDeleteConfirmation;
        private bool _languageDeleteConfirmation;
        private bool _categoryDeleteConfirmation;

        private int _searchTypeIndex;

        private bool _showLogs;
        
        #endregion

        #region DIMENSION VARIABLES
        
        #endregion

        #endregion

        #region PUBLIC METHODS

        public void ShowLayout()
        {
            GUILayout.BeginVertical(MinHeightOption(_windowSize.y));
            
            Title();

            ShowHorizontalLine(5);
            
            GUILayout.BeginHorizontal();
            
            OptionsSection();

            ShowVerticalLine(5);
            
            ExportSection();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }



        #endregion

        #region PRIVATE METHODS

        private void Title()
        {
	        GUILayout.Space(15);

	        ShowHeader("CONFIGURATION");

	        GUILayout.Space(10);
        }
        
        private void OptionsSection()
        {
	        var style = new GUIStyle
	        {
		        margin = new RectOffset(100, 100, 10, 25)
	        };
	        
	        GUILayout.BeginVertical(style);
	        
	        DeleteConfirmationSection();

	        SearchSection();

	        LogsSection();

	        ReadmeSection();

	        GUILayout.EndVertical();
        }

        private void ExportSection()
        {
	        GUILayout.BeginVertical(MinWidthOption(_windowSize.x * 0.5f));

	        ShowSectionHeader("EXPORT - IMPORT");

	        GUILayout.EndVertical();
        }

        private void DeleteConfirmationSection()
        {
	        ShowSubHeader("Delete Confirmation");

	        if (_dictionaryDeleteConfirmation && _languageDeleteConfirmation && _categoryDeleteConfirmation) _toggleAllDeleteConfirmation = true;
	        else _toggleAllDeleteConfirmation = false;	
	        
			var tempToogle = EditorGUILayout.Toggle(_toggleAllDeleteConfirmation);
			UpdateAllDeleteConfirmation(_toggleAllDeleteConfirmation, tempToogle);
	        
	        _dictionaryDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in DICTIONARY", LocalizationManager.Configuration.DictionaryDeleteConfirmation, MinHeightOption(24));
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Dictionary);

	        _languageDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in LANGUAGES", LocalizationManager.Configuration.LanguageDeleteConfirmation, MinHeightOption(24));
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Languages);

	        _categoryDeleteConfirmation = EditorGUILayout.ToggleLeft("Show delete confirmation in CATEGORIES", LocalizationManager.Configuration.CategoryDeleteConfirmation, MinHeightOption(24));
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Categories);
        }

        private void SearchSection()
        {
	        ShowSubHeader("Search");
	        _searchTypeIndex = EditorGUILayout.Popup(LocalizationManager.Configuration.SearchTypeIndex, Enums.SEARCH_TYPE, SearchTypeStyle());
	        UpdateSearchType();
	        GUILayout.Space(5);
        }

        private void LogsSection()
        {
	        ShowSubHeader("Logs");
	        _showLogs = EditorGUILayout.ToggleLeft("Show logs in Console", LocalizationManager.Configuration.ShowLogsInConsole, MinHeightOption(24));
	        UpdateShowLogs();
        }

        private void ReadmeSection()
        {
	        ShowSubHeader("Readme");
	        //TODO: poner boton para abrir readme/documentation o link
        }

        #region UPDATES

        private void UpdateAllDeleteConfirmation(bool oldValue, bool newValue)
        {
	        if (oldValue == newValue) return;
	        _toggleAllDeleteConfirmation = newValue;
	        _dictionaryDeleteConfirmation = newValue;
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Dictionary);
	        
	        _languageDeleteConfirmation = newValue;
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Languages);

	        _categoryDeleteConfirmation = newValue;
	        UpdateDeleteConfirmation(Enums.GUI_WINDOW.Categories);
        }
        
        private void UpdateDeleteConfirmation(Enums.GUI_WINDOW window)
        {
	        switch (window)
	        {
		        case Enums.GUI_WINDOW.Dictionary:
			        LocalizationManager.Instance.UpdateDeleteConfirmation(_dictionaryDeleteConfirmation, window);
			        break;
		        case Enums.GUI_WINDOW.Languages:
			        LocalizationManager.Instance.UpdateDeleteConfirmation(_languageDeleteConfirmation, window);
			        break;
		        case Enums.GUI_WINDOW.Categories:
			        LocalizationManager.Instance.UpdateDeleteConfirmation(_categoryDeleteConfirmation, window);
			        break;
	        }
        }

        private void UpdateSearchType()
        {
	        LocalizationManager.Instance.UpdateSearchType(_searchTypeIndex);
        }

        private void UpdateShowLogs()
        {
	        LocalizationManager.Instance.UpdateShowLog(_showLogs);
        }
        
        #endregion

        #endregion
	}
}
using System.Linq;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.General;
using LocalizationTool.Scripts.Serializer;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace UIManager
{    
	[DefaultExecutionOrder(100)]
	public class UIManager : MonoBehaviour
	{
		#region Inspector Variables

		[Header("Main Menu")]
		[SerializeField] private GameObject mainMenuGroup;

		[SerializeField] private LocalizationToolAddon playButton;
		[SerializeField] private LocalizationToolAddon optionsButton;
		[SerializeField] private LocalizationToolAddon exitButton;
		
		[Header("Options")]
		[SerializeField] private GameObject optionsGroup;
		[SerializeField] private LocalizationToolAddon optionsTitle;
		[SerializeField] private TMP_Dropdown tmpDropdown;
		[SerializeField] private LocalizationToolAddon volumeLabel;
		[SerializeField] private LocalizationToolAddon backButton;
		
		#endregion

		#region Public Variables
		
		#endregion

		#region Private Variables

		private const string BACKUP_PATH = "Assets/LocalizationTool/Scripts/Data/Backups/CurrentBackup.json";
		private const string DEMO_PATH = "Assets/LocalizationTool/Demo/Data/DemoData.json";
		
		#endregion

		#region Unity Methods

		private void Awake()
		{
			mainMenuGroup.SetActive(true);
			optionsGroup.SetActive(false);
			LocalizationManager.Instance.OnLocalizationToolInitialized += ToolInitialized;
		}
		
		private void OnDisable()
		{
			LocalizationManager.Instance.OnLocalizationToolInitialized -= ToolInitialized;
		
			LoadBackupData();
		}
		

		#endregion

		#region Public Methods

		public void GoToOptions()
		{
			mainMenuGroup.SetActive(false);
			optionsGroup.SetActive(true);
		}

		public void GoToMainMenu()
		{
			mainMenuGroup.SetActive(true);
			optionsGroup.SetActive(false);
		}

		#endregion

		#region Private Methods

		private void ToolInitialized()
		{
			LocalizationManager.SaveFile(BACKUP_PATH, LocalizationManager.BuildSerializedData(new UnityJsonSerializer()), "", "", "");
			Debug.Log("Data saved to backup");
			
			LocalizationManager.Instance.ClearData();
			LoadDemoData();
		}

		private static void OnLanguageUpdate(int index)
		{
			var language = LocalizationToolAPI.GetAvailableLanguages()[index];
			LocalizationToolAPI.ChangeLanguage(language);
		}

		private void LoadDemoData()
		{
			var currentLogValue = LocalizationManager.Configuration.showLogsInConsole;
			LocalizationManager.Configuration.showLogsInConsole = false;

			StartCoroutine(LocalizationManager.ImportSerializedDataCoroutine(DEMO_PATH, new UnityJsonSerializer(), null,
				() =>
				{
					UpdateAllItems();
					LocalizationManager.Configuration.showLogsInConsole = currentLogValue;
				}));
			Debug.Log("Data uploaded from DemoData");
		}

		private void UpdateAllItems()
		{
			playButton.SetKey("Play_Button");
			optionsButton.SetKey("Options_Button");
			exitButton.SetKey("Exit_Button");
			optionsTitle.SetKey("Options_Title");
			volumeLabel.SetKey("Volume_Label");
			backButton.SetKey("Back_Button");
			
			var languages = LocalizationToolAPI.GetAvailableLanguages();

			tmpDropdown.options = languages.Select(language => new TMP_Dropdown.OptionData(language)).ToList();
			tmpDropdown.onValueChanged.AddListener(OnLanguageUpdate);
			LocalizationToolAPI.ChangeLanguage("English");
		}
		
		private static void LoadBackupData()
		{
			LocalizationManager.Instance.ClearData();
			var currentLogValue = LocalizationManager.Configuration.showLogsInConsole;
			LocalizationManager.Configuration.showLogsInConsole = false;
			EditorCoroutineUtility.StartCoroutineOwnerless(LocalizationManager.ImportSerializedDataCoroutine(BACKUP_PATH, new UnityJsonSerializer(), null,
				() => LocalizationManager.Configuration.showLogsInConsole = currentLogValue));
			Debug.Log("Reload data from backup");
		}

		#endregion
	}
}
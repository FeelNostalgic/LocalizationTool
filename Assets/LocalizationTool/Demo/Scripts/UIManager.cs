#if UNITY_EDITOR
using System.Linq;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.General;
using LocalizationTool.Scripts.Serializer;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace LocalizationTool.Demo.Scripts
{
    [DefaultExecutionOrder(-999)]
    public class UIManager : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Main Menu")] [SerializeField] private GameObject mainMenuGroup;

        [SerializeField] private LocalizationToolAddon playButton;
        [SerializeField] private LocalizationToolAddon optionsButton;
        [SerializeField] private LocalizationToolAddon exitButton;

        [Header("Options")] [SerializeField] private GameObject optionsGroup;
        [SerializeField] private LocalizationToolAddon optionsTitle;
        [SerializeField] private TMP_Dropdown tmpDropdown;
        [SerializeField] private LocalizationToolAddon volumeLabel;
        [SerializeField] private LocalizationToolAddon backButton;

        #endregion
        
        #region Private Variables

        private const string BACKUP_PATH = "Assets/LocalizationTool/PersistentData/Backups/CurrentBackup.json";
        private const string DEMO_PATH = "Assets/LocalizationTool/Demo/Data/DemoData.json";

        #endregion

        #region Unity Methods

        private void Awake()
        {
            mainMenuGroup.SetActive(true);
            optionsGroup.SetActive(false);
            CacheDataSO.OnLocalizationToolDataInitialized += ToolInitialized;
        }

        private void OnDisable()
        {
            CacheDataSO.OnLocalizationToolDataInitialized -= ToolInitialized;

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
            SaveLoadFileManager.SaveFile(BACKUP_PATH, LocalizationManager.BuildSerializedData(new UnityJsonSerializer()));
            Debug.Log("Data saved to backup");

            CacheDataSO.ClearData();
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
            
            StartCoroutine(LocalizationManager.ImportSerializedDataCoroutineForGameMode(DEMO_PATH,
                () =>
                {
                    UpdateAllItems();
                    LocalizationManager.Configuration.showLogsInConsole = currentLogValue;
                }));

            Debug.Log("Data uploaded from DemoData");
        }

        private static void LoadBackupData()
        {
            CacheDataSO.ClearData();

            var currentLogValue = LocalizationManager.Configuration.showLogsInConsole;
            LocalizationManager.Configuration.showLogsInConsole = false;

            EditorCoroutineUtility.StartCoroutineOwnerless(LocalizationManager.ImportSerializedDataCoroutineForGameMode(BACKUP_PATH,
                () => LocalizationManager.Configuration.showLogsInConsole = currentLogValue));
            Debug.Log("Reload data from backup");
        }

        private void UpdateAllItems()
        {
            playButton.SetKey("Play_Button");
            optionsButton.SetKey("Options_Button");
            exitButton.SetKey("Exit_Button");
            optionsTitle.SetKey("Options_Title");
            volumeLabel.SetKey("Volume_Label");
            backButton.SetKey("Back_Button");

            // Update language selector
            var languages = LocalizationToolAPI.GetAvailableLanguages();
            tmpDropdown.options = languages.Select(language => new TMP_Dropdown.OptionData(language)).ToList();
            tmpDropdown.onValueChanged.AddListener(OnLanguageUpdate);
            tmpDropdown.value = 1;

            LocalizationToolAPI.ChangeLanguage("English");
        }

        #endregion
    }
}
#endif
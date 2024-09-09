using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using TMPro;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;

namespace LocalizationTool.Scripts.Addons
{
    [AddComponentMenu("Localization Tool/Addon", 2)]
    [RequireComponent(typeof(TMP_Text))]
    [DefaultExecutionOrder(-999)]
    public class LocalizationToolAddon : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public string Key
        {
            get => key;
            set => key = value;
        }

        public int KeyIndex
        {
            get => keyIndex;
            set => keyIndex = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        [SerializeField] private string key;
        [SerializeField] private int keyIndex;

        private TMP_Text _tmpText;

        #endregion

        #region UNITY METHODS

        //TODO: Instead of awake, maybe OnEnable is needed
        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();

            if (!LocalizationToolAPI.Instance.IsNull()) return;
            Debug.LogError($"{ADDON_WARNING_API_NOT_INIT_1}. {ADDON_WARNING_API_NOT_INIT_2}");
        }

        #endregion

        #region PUBLIC METHODS

        public void SetKey(string keyToSet)
        {
            key = keyToSet;
            keyIndex = LocalizationToolAPI.GetAllKeys().IndexOf(key);
            var newText = LocalizationToolAPI.GetValueByKey(key, out var found);
            if (_tmpText.IsNull()) _tmpText = GetComponent<TMP_Text>();
            if (found) _tmpText.text = newText;
        }
        
        public void OnKeyRemoved(string keyToRemove)
        {
            if(key.IsNull()) return;
            if (!key.Equals(keyToRemove)) return;
            key = "";
            keyIndex = -1;
        }

        public void OnKeyUpdate(string oldKey, string newKey)
        {
            if(key.IsNull()) return;
            if (oldKey.Equals(key))
            {
                key = newKey;
            }
        }
        
        public void LanguageUpdate(string newLanguage)
        {
            var newText = LocalizationToolAPI.GetValueByKey(key, out _);
            //Debug.Log($"Key: '{key}' => new language: '{newLanguage}' => value: {newText}");
            if (_tmpText.IsNull()) _tmpText = GetComponent<TMP_Text>();
            _tmpText.text = newText;
        }

        #endregion
    }
}
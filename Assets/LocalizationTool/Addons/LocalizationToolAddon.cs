using LocalizationTool.Controller;
using TMPro;
using UnityEngine;

namespace LocalizationTool.Addons
{
    [RequireComponent(typeof(TMP_Text))]
    [DefaultExecutionOrder(-999)]
    public class LocalizationToolAddon : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public int KeyIndex
        {
            get => _keyIndex;
            set => _keyIndex = value;
        }

        #endregion

        #region PRIVATE VARIABLES

        [SerializeField] private string _key;
        [SerializeField] private int _keyIndex;

        private TMP_Text _tmpText;

        #endregion

        #region UNITY METHODS

        //TODO: Instead of awake, maybe OnEnable is needed
        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            LocalizationToolController.Instance.OnLanguageUpdate += OnLanguageUpdate;
        }

        private void OnDestroy()
        {
            LocalizationToolController.Instance.OnLanguageUpdate -= OnLanguageUpdate;
        }

        public void OnKeyRemoved(string key)
        {
            if (!_key.Equals(key)) return;
            _key = "";
            _keyIndex = -1;
        }

        public void OnKeyUpdate(string oldKey, string newKey)
        {
            if (oldKey.Equals(_key))
            {
                _key = newKey;
            }
        }
        
        #endregion

        #region PRIVATE METHODS
        
        private void OnLanguageUpdate(string newLanguage)
        {
            var newText = LocalizationToolController.Instance.GetValueByKey(Key);
            _tmpText.text = newText;
        }

        #endregion
    }
}
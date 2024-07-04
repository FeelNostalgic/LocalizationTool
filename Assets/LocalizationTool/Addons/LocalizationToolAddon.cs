using System;
using LocalizationTool.Controller;
using TMPro;
using UnityEngine;

namespace LocalizationTool.Addons
{
    [RequireComponent(typeof(TMP_Text))]
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

        #endregion

        #region PRIVATE METHODS

        private void OnLanguageUpdate(string newLanguage)
        {
            _tmpText.text = LocalizationToolController.Instance.GetValueByKey(Key);
        }

        #endregion
    }
}
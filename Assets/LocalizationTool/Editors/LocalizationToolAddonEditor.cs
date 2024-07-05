using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Addons;
using LocalizationTool.Controller;
using LocalizationTool.Manager;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(LocalizationToolAddon))]
    public class LocalizationToolAddonEditor : Editor
    {
        #region PRIVATE VARIABLES

        private int _keyIndex = 0;
        private int _filterCategoryIndex = 0;
        private string _filterKeyText = "";
        
        private LocalizationToolAddon _target;

        #endregion

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            _target = (LocalizationToolAddon)target;
            
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Localization Tool", HeaderStyle());

            GUILayout.Space(8);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(8);

            //Category Filter
            var allCategories = LocalizationToolController.Instance.GetAllCategories();
            _filterCategoryIndex = EditorGUILayout.Popup(_filterCategoryIndex, allCategories.ToArray(), PopupStyle());
            
            GUILayout.Space(8);

            //Text key filter
            _filterKeyText = EditorGUILayout.TextField(_filterKeyText, TextFieldStyle());

            GUILayout.Space(10);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(8);

            var allKeys = LocalizationToolController.Instance.GetAllKeys();
            var filteredKeys = new List<string>(allKeys);

            if (!_filterKeyText.Equals(""))
                filteredKeys = filteredKeys.Where(key => key.Contains(_filterKeyText, StringComparison.InvariantCulture)).ToList();

            if (_filterCategoryIndex != 0)
                filteredKeys = filteredKeys.Where(key => LocalizationManager.Dictionary[key].Category == allCategories[_filterCategoryIndex]).ToList();
            
            GUILayout.Label("KEY", KeyLabelStyle());
            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            _keyIndex = EditorGUILayout.Popup(_target.KeyIndex, filteredKeys.ToArray(), PopupStyle());
            if (EditorGUI.EndChangeCheck())
            {
                if (filteredKeys.Count > 0)
                {
                    UpdateKey(allKeys[_keyIndex]);
                }
            }

            GUILayout.Space(12);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(5);

            // Selected key-value label
            GUILayout.Label(_target.Key, SelectedKeyLabelStyle());
            GUILayout.Space(5);
            try
            {
                GUILayout.Label(LocalizationToolController.Instance.GetValueByKey(_target.Key), SelectedValueLabelStyle());
            }
            catch (Exception)
            {
                GUILayout.Label("", SelectedValueLabelStyle());
            }
            
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);
            
        }

        #region PRIVATE METHODS

        #region UPDATES

        private void UpdateKey(string newKey)
        {
            if (newKey.Equals(_target.Key)) return;
            
            Undo.RecordObject(_target, "Change Key");
            _target.Key = newKey;
            _target.KeyIndex = _keyIndex;
            serializedObject.Update();
            EditorUtility.SetDirty(_target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_target);
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"Key update to '{newKey}'");
        }

        #endregion

        #region STYLES

        private static GUIStyle HeaderStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 15
            };
            return style;
        }

        private static GUIStyle KeyLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            return style;
        }

        private static GUIStyle PopupStyle()
        {
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24,
                fontSize = 13,
                normal =
                {
                    textColor = Color.white
                }
            };
            return style;
        }

        private static GUIStyle TextFieldStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fixedHeight = 24,
                normal =
                {
                    textColor = Color.white
                }
            };
            return style;
        }

        private static GUIStyle SelectedKeyLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fixedHeight = 24,
                normal =
                {
                    textColor = Color.white
                }
            };
            return style;
        }
        
        private static GUIStyle SelectedValueLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 13,
                fixedHeight = 24,
                normal =
                {
                    textColor = Color.white
                }
            };
            return style;
        }

        #endregion

        #endregion
    }
#endif
}
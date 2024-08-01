using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Addons;
using LocalizationTool.Commons;
using LocalizationTool.Controller;
using LocalizationTool.Manager;
using LocalizationTool.Data;
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
        private Vector2 _scrollView;

        private LocalizationToolAddon _target;

        #endregion

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            _target = (LocalizationToolAddon)target;

            
            EditorGUILayout.BeginVertical(); 
            Row1();

            GUILayout.Space(2);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(2);

            var allCategories = Row2();

            GUILayout.Space(2);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(2);

            Row3(allCategories);

            GUILayout.Space(2);
            LocalizationEditor.ShowHorizontalLine(4);
            GUILayout.Space(2);
            
            Row4();

            EditorGUILayout.EndVertical(); 
        }

        private static void Row1()
        {
            GUILayout.Label("Localization Tool", CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label), GUILayout.Height(30));
        }

        private List<string> Row2()
        {
            GUILayout.BeginVertical("box", GUILayout.Height(35*2+5+3), GUILayout.ExpandWidth(true));
            GUILayout.Space(2);
            var allCategories = LocalizationToolController.Instance.GetAllCategories();
            //Text key filter
            
            _filterKeyText = EditorGUILayout.TextField(_filterKeyText, CustomStyles.GetStyle(Enums.CustomStyleName.KeyTextField), GUILayout.Height(30));
             
            GUILayout.Space(5);
            _filterCategoryIndex = EditorGUILayout.Popup(_filterCategoryIndex, allCategories.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
            
            GUILayout.EndVertical();
            return allCategories;
        }

        private void Row3(IReadOnlyList<string> allCategories)
        {
            var allKeys = GetFilteredKeys(allCategories, out var filteredKeys);

            GUILayout.BeginVertical("box", GUILayout.Height(30*2+5+4), GUILayout.ExpandWidth(true));

            GUILayout.Label("SELECT KEY", CustomStyles.GetStyle(Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel), GUILayout.Height(25));
            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            _keyIndex = EditorGUILayout.Popup(_target.KeyIndex, filteredKeys.ToArray(), CustomStyles.GetStyle(Enums.CustomStyleName.CategoryPopup));
            if (EditorGUI.EndChangeCheck())
            {
                if (filteredKeys.Count > 0)
                {
                    UpdateKey(allKeys[_keyIndex]);
                }
            }

            GUILayout.EndVertical();
        }

        private void Row4()
        {
            // Selected key-value label
            GUILayout.BeginVertical("box", GUILayout.Height(30+45+5+4), GUILayout.ExpandWidth(true));
            GUILayout.Space(2);
            var key = new GUIContent(_target.Key, "Selected Key");
            GUILayout.Label(key, CustomStyles.GetStyle(Enums.CustomStyleName.KeySelectableLabel), GUILayout.Height(30));
            GUILayout.Space(5);
            _scrollView = GUILayout.BeginScrollView(_scrollView, GUILayout.ExpandWidth(true), GUILayout.Height(45));

            var value = "";
            try
            {
                value = LocalizationToolController.Instance.GetValueByKey(_target.Key);
            }
            catch (Exception)
            {
                // ignored
            }

            GUILayout.Label(value, CustomStyles.GetStyle(Enums.CustomStyleName.AddonSelectableTextFieldLabel), GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
            
            GUILayout.EndScrollView();
            
            GUILayout.Space(2);

            GUILayout.EndVertical();
        }

        private List<string> GetFilteredKeys(IReadOnlyList<string> allCategories, out List<string> filteredKeys)
        {
            var allKeys = LocalizationToolController.Instance.GetAllKeys();
            filteredKeys = new List<string>(allKeys);

            if (!_filterKeyText.Equals(""))
                filteredKeys = filteredKeys.Where(key => key.Contains(_filterKeyText, StringComparison.InvariantCulture)).ToList();
            
            if (_filterCategoryIndex != 0)
                filteredKeys = filteredKeys.Where(key => LocalizationManager.Dictionary[key].Category == allCategories[_filterCategoryIndex]).ToList();
            return filteredKeys;
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
        }

        #endregion

        #endregion
    }
#endif
}
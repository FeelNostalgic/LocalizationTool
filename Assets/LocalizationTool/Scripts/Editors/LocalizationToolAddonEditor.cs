#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data;
using UnityEditor;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Editors.EditorWindowAbstract;

namespace LocalizationTool.Scripts.Editors
{
    [CustomEditor(typeof(LocalizationToolAddon))]
    public class LocalizationToolAddonEditor : Editor
    {
        #region PRIVATE VARIABLES

        private int _keyIndex = -1;
        private int _filterCategoryIndex = 0;
        private string _filterKeyText = "";
        private Vector2 _scrollView;

        private LocalizationToolAddon _target;

        #endregion

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            if (LocalizationToolAPI.Instance.IsNull())
            {
                _target = (LocalizationToolAddon)target;

                GUILayout.BeginVertical("box", GUILayout.Height(30+25+60+2+2)); 
                Row1();
                GUILayout.Space(2);
                ShowHorizontalLine(4);
                GUILayout.Space(2);
                
                GUILayout.BeginVertical();
                
                //ICON
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var icon = EditorGUIUtility.IconContent("d_console.warnicon.sml").image;
                GUILayout.Label(icon, GUILayout.Height(25));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                //LABEL
                var style = new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.MiddleCenter, fontSize = 13};
                GUILayout.Label(ADDON_WARNING_API_NOT_INIT_1, style, GUILayout.Height(30), GUILayout.ExpandHeight(true));
                GUILayout.Label(ADDON_WARNING_API_NOT_INIT_2, style, GUILayout.Height(30), GUILayout.ExpandHeight(true));
                
                GUILayout.EndVertical();
                
                GUILayout.EndVertical(); 
            }
            else
            {
                _target = (LocalizationToolAddon)target;

                GUILayout.BeginVertical(); 
                Row1();

                GUILayout.Space(2);
                ShowHorizontalLine(4);
                GUILayout.Space(2);

                var allCategories = Row2();

                GUILayout.Space(2);
                ShowHorizontalLine(4);
                GUILayout.Space(2);

                Row3(allCategories);

                GUILayout.Space(2);
                ShowHorizontalLine(4);
                GUILayout.Space(2);
            
                Row4();

                GUILayout.EndVertical(); 
            }
        }

        private static void Row1()
        {
            GUILayout.Label(MAIN_WINDOW_LABEL, CustomStyles.GetStyle(Enums.CustomStyleName.Header2BoldMiddleCenter15Label), GUILayout.Height(30));
        }

        private List<string> Row2()
        {
            GUILayout.BeginVertical("box", GUILayout.Height(35*2+5+3), GUILayout.ExpandWidth(true));
            GUILayout.Space(2);
            var allCategories = LocalizationToolAPI.GetAllCategories();
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

            GUILayout.Label(ADDON_SELECT_KEY_LABEL, CustomStyles.GetStyle(Enums.CustomStyleName.ColumnsTitleBoldMiddleLeftLabel), GUILayout.Height(25));
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
            GUILayout.Label(new GUIContent(_target.Key, ADDON_SELECTED_KEY_TOOLTIP), CustomStyles.GetStyle(Enums.CustomStyleName.KeySelectableLabel), GUILayout.Height(30));
            GUILayout.Space(5);
            _scrollView = GUILayout.BeginScrollView(_scrollView, GUILayout.ExpandWidth(true), GUILayout.Height(45));

            var value = LocalizationToolAPI.GetValueByKey(_target.Key, out _);

            GUILayout.Label(value, CustomStyles.GetStyle(Enums.CustomStyleName.AddonSelectableTextFieldLabel), GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
            
            GUILayout.EndScrollView();
            
            GUILayout.Space(2);

            GUILayout.EndVertical();
        }

        private List<string> GetFilteredKeys(IReadOnlyList<string> allCategories, out List<string> filteredKeys)
        {
            var allKeys = LocalizationToolAPI.GetAllKeys();
            if (allKeys.IsNull())
            {
                filteredKeys = new List<string>();
                return null;
            }
            
            filteredKeys = new List<string>(allKeys);

            if (!_filterKeyText.Equals(""))
                filteredKeys = filteredKeys.Where(key => key.Contains(_filterKeyText, StringComparison.InvariantCulture)).ToList();
            
            if (_filterCategoryIndex != 0)
                filteredKeys = filteredKeys.Where(key => CacheDataSO.localizationData.KeysDictionary[key].category.name == allCategories[_filterCategoryIndex]).ToList();
            return filteredKeys;
        }

        #region PRIVATE METHODS

        #region UPDATES

        private void UpdateKey(string newKey)
        {
            if (newKey.Equals(_target.Key)) return;

            Undo.RecordObject(_target, $"Changed Key Selected to {newKey}");
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
}
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.Data.TemplatesForSerializer;
using LocalizationTool.Scripts.Editors;
using LocalizationTool.Scripts.General;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LocalizationTool.Scripts.Data
{
    public static class CacheDataSO
    {
        #region PUBLIC VARIABLES

        //public static CacheDataSO Instance => _instance ??= new CacheDataSO();
        public static bool IsDataLoaded { get; private set; }

        public static List<KeyDataSO> Keys => localizationData.keys;

        public static List<LanguageDataSO> LanguageCache { get; private set; }
        public static List<string> Languages => LanguageCache.Select(x => x.languageName).ToList();
        public static string DefaultLanguage => LanguageCache.FirstOrDefault(x => x.isDefault)?.languageName;

        public static List<CategoryDataSO> CategoryCache { get; private set; }
        public static List<string> Categories => CategoryCache.Select(x => x.categoryName).ToList();
        public static string DefaultCategory => CategoryCache.FirstOrDefault(x => x.isDefault)?.categoryName;

        public static Action OnLocalizationToolDataInitialized { get; set; }

        #endregion

        #region PRIVATE VARIABLES

        //private static CacheDataSO _instance;
        public static LocalizationDataSO localizationData;

        private const string DATABASE_PATH = "Assets/LocalizationTool/Database";
        private const string LANGUAGES_PATH = DATABASE_PATH + "/Languages";
        private const string CATEGORIES_PATH = DATABASE_PATH + "/Categories";
        private const string KEYS_PATH = DATABASE_PATH + "/Keys";
        private const string LOCALIZATION_DATA_PATH = DATABASE_PATH + "/LocalizationData.asset";
        
        // Undo Tracker
        
        private static readonly Stack<(string, string)> UndoRenameKeyTracker = new();
        private static readonly Stack<(string, string)> UndoRenameCategoryTracker = new();
        private static readonly Stack<(string, string)> UndoRenameLanguageTracker = new();
        
        private static readonly Stack<(int, LanguageTemplate)> UndoRemoveLanguageTracker = new(); 
        private static readonly Stack<(CategoryDataSO, List<KeyDataSO>)> UndoRemoveCategoryTracker = new(); 

        #endregion

        #region LOAD CACHE

        public static void InitForEditor(Action onComplete = null)
        {
            if (IsDataLoaded) return;

            LoadCacheData();
            IsDataLoaded = true;
            onComplete?.Invoke();
        }

        public static void LoadCacheData()
        {
            if (IsDataLoaded) return;

            CreateOrLoadLocalizationData();

            UpdateLanguageCache();

            UpdateCategoriesCache();

#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif

            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;

            IsDataLoaded = true;

            OnLocalizationToolDataInitialized?.Invoke();
        }

        public static void LoadLanguagesCache()
        {
            CreateOrLoadLocalizationData();
            UpdateLanguageCache();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;
        }

        public static void LoadCategoriesCache()
        {
            CreateOrLoadLocalizationData();
            UpdateCategoriesCache();
        }

        public static void LoadKeysCache()
        {
            CreateOrLoadLocalizationData();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
        }

        public static void ClearData()
        {
            var tempKList = new List<KeyDataSO>(Keys);
            tempKList.ForEach(x => RemoveKey(x.keyName));

            var tempLList = new List<LanguageDataSO>(LanguageCache);
            tempLList.ForEach(x => RemoveLanguage(x.languageName));

            var tempCList = new List<CategoryDataSO>(CategoryCache);
            tempCList.ForEach(x => RemoveCategory(x.categoryName));
        }

        #endregion

        #region KEYS

        public static void InsertKey(string keyName, string categoryName)
        {
            CreateDirectoryIfNotExist(KEYS_PATH);

            var newKeyData = ScriptableObject.CreateInstance<KeyDataSO>();
            AssetDatabase.CreateAsset(newKeyData, $"{KEYS_PATH}/{keyName}.asset");
            newKeyData.keyName = keyName;
            newKeyData.displayOrder = localizationData.keys.Count + 1;
            newKeyData.category = CategoryCache.FirstOrDefault(x => x.categoryName.Equals(categoryName));
            localizationData.keys.Add(newKeyData);

            foreach (var languageData in LanguageCache)
            {
                var translation = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
                translation.keyData = newKeyData;
                languageData.translationKeys.Add(translation);
                AssetDatabase.AddObjectToAsset(translation, languageData);
            }

            SaveChanges();
        }

        public static void RemoveKey(string keyName)
        {
            // FUTURE: ADD UNDO
            var keyToRemove = localizationData.KeysDictionary[keyName];
            localizationData.keys.Remove(keyToRemove);
            RemoveAsset(keyToRemove);

            foreach (var localizationDataLanguage in localizationData.languages)
            {
                var translationToRemove = localizationDataLanguage.TranslationDictionary[keyName];
                localizationDataLanguage.translationKeys.Remove(translationToRemove);
                AssetDatabase.RemoveObjectFromAsset(translationToRemove);
                Object.DestroyImmediate(translationToRemove, true);
            }

            SaveChanges();
        }

        public static void UpdateKeyCategory(string key, string newCategory)
        {
            var keyToUpdate = localizationData.KeysDictionary[key];
            Undo.RecordObject(keyToUpdate, $"Key {key} updated category to {newCategory}");
            keyToUpdate.category = localizationData.CategoriesDictionary[newCategory];

            SaveChanges();
        }

        public static void SetKeyTranslation(string key, string newTranslation, string language, bool performUndo = true)
        {
            var translationToUpdate = localizationData.LanguagesDictionary[language].TranslationDictionary[key];
            Undo.RecordObject(translationToUpdate, $"Change translation {key} in {language} to {newTranslation}");
            translationToUpdate.translationText = newTranslation;
            
            SaveChanges();
            
            if (!performUndo) return;
            Undo.undoRedoPerformed -= DictionaryEditor.RefreshTextBuffer;
            Undo.undoRedoPerformed += DictionaryEditor.RefreshTextBuffer;
            Undo.undoRedoPerformed -= RichTextEditor.RefreshTextArea;
            Undo.undoRedoPerformed += RichTextEditor.RefreshTextArea;
        }

        public static void UpdateKeyName(string oldKeyName, string newKeyName)
        {
            var keyToUpdate = localizationData.keys.FirstOrDefault(x => x.keyName.Equals(oldKeyName));
            Undo.RecordObject(keyToUpdate, $"Rename Key '{oldKeyName}' to '{newKeyName}'");
            UndoRenameKeyTracker.Push((oldKeyName, newKeyName));
            
            keyToUpdate!.keyName = newKeyName;
            EditorUtility.SetDirty(keyToUpdate);
            AssetDatabase.RenameAsset(GetPath(keyToUpdate), newKeyName);
            
            SaveChanges();

            Undo.undoRedoPerformed -= HandleUndoRedoUpdateKeyName;
            Undo.undoRedoPerformed += HandleUndoRedoUpdateKeyName;
        }

        private static void HandleUndoRedoUpdateKeyName()
        {
            if (UndoRenameKeyTracker.IsEmpty()) return;
            var previousCurrentName = UndoRenameKeyTracker.Pop();
            
            var assetPath = $"{KEYS_PATH}/{previousCurrentName.Item2}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath).IsNull()) return;
            AssetDatabase.RenameAsset(assetPath, previousCurrentName.Item1);
            
            RichTextEditor.key = previousCurrentName.Item1;
            RichTextEditor.Refresh();
            SaveChanges();
        }

        #endregion

        #region LANGUAGES

        public static void InsertLanguage(string newLanguageName)
        {
            var newLanguage = ScriptableObject.CreateInstance<LanguageDataSO>();
            newLanguage.languageName = newLanguageName;
            newLanguage.displayOrder = localizationData.languages.Count + 1;
            if (localizationData.languages.Count == 0) newLanguage.isDefault = true;
            AssetDatabase.CreateAsset(newLanguage, $"{LANGUAGES_PATH}/{newLanguageName}.asset");

            localizationData.languages.Add(newLanguage);

            foreach (var keyData in localizationData.keys)
            {
                var translation = ScriptableObject.CreateInstance<TranslationKeyDataSO>();
                translation.name = keyData.keyName;
                translation.keyData = keyData;
                newLanguage.translationKeys.Add(translation);
                AssetDatabase.AddObjectToAsset(translation, newLanguage);
            }

            UpdateLanguageCache();
            SaveChanges();
        }

        public static void SetDefaultLanguage(string newDefaultLanguage)
        {
            // Remove old default
            var currentDefault = LanguageCache[0];
            Undo.RecordObject(currentDefault, $"Change default language to {newDefaultLanguage}");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = localizationData.LanguagesDictionary[newDefaultLanguage];
            Undo.RecordObject(newDefault, $"Change default language to {newDefaultLanguage}");
            newDefault.isDefault = true;

            // Reorder languages
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var language in localizationData.languages.Where(language => language.displayOrder < newDefaultOrder))
            {
                Undo.RecordObject(language, $"Change default language to {newDefaultLanguage}");
                language.displayOrder++; // Shift languages above the new default
            }

            newDefault.displayOrder = 1;

            UpdateLanguageCache();
            SaveChanges();
            
            Undo.undoRedoPerformed -= UpdateLanguagesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateLanguagesCacheAndRepaint;
        }

        public static void RemoveLanguage(string languageNameToRemove)
        {
            var languageToRemove = localizationData.LanguagesDictionary[languageNameToRemove];
            var removedDisplayOrder = languageToRemove!.displayOrder;

            // FUTURE: Undo removed language
            // var undoGroup = Undo.GetCurrentGroup();
            // Undo.SetCurrentGroupName($"Remove language {languageToRemove.languageName}");
            
            // UndoRemoveLanguageTracker.Push((languageToRemove.displayOrder, CreateLanguageCopy(languageToRemove)));
            
            // Update display order
            foreach (var language in localizationData.languages.Where(language => language.displayOrder > removedDisplayOrder))
            {
                language.displayOrder--;
            }

            localizationData.languages.Remove(languageToRemove);

            foreach (var translationKeyData in languageToRemove.translationKeys)
            {
                AssetDatabase.RemoveObjectFromAsset(translationKeyData);
                Object.DestroyImmediate(translationKeyData, true);
            }

            RemoveAsset(languageToRemove);
            
            // FUTURE: Undo removed language
            // Undo.CollapseUndoOperations(undoGroup);
            
            UpdateLanguageCache();
            SaveChanges();

            if (LocalizationManager.CurrentLanguageInDictionarySection.Equals(languageNameToRemove))
            {
                LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
            }
            
            // FUTURE: Undo removed language
            // Undo.undoRedoPerformed -= HandleUndoRedoRemovedLanguage;
            // Undo.undoRedoPerformed += HandleUndoRedoRemovedLanguage;
        }

        // FUTURE: Undo removed language
        // private static LanguageTemplate CreateLanguageCopy(LanguageDataSO languageToCopy)
        // {
        //     var copiedData = new LanguageTemplate
        //     {
        //         Language = languageToCopy.languageName
        //     };
        //     
        //     foreach (var keyData in Keys)
        //     {
        //         var key = keyData.keyName;
        //         var category = keyData.CategoryName;
        //         
        //         copiedData.Data.Add(new LanguageTemplate.KeyCategoryLanguage
        //         {
        //             Key = key,
        //             Category = category,
        //             Value = languageToCopy.TranslationDictionary[key].translationText
        //         });
        //     }
        //     
        //     return copiedData;
        // }
        //
        // private static void LoadSavedLanguage(LanguageTemplate languageToLoad)
        // {
        //     LocalizationManager.ImportLanguage(languageToLoad.Language);
        //     
        //     foreach (var keyCategoryLanguage in languageToLoad.Data)
        //     {
        //         LocalizationManager.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, languageToLoad.Language, keyCategoryLanguage.Value);
        //         //yield return null;
        //     }
        // }
        //
        // private static void HandleUndoRedoRemovedLanguage()
        // {
        //     if (UndoRemoveLanguageTracker.IsEmpty()) return;
        //     var removedLanguage = UndoRemoveLanguageTracker.Pop();
        //     // EditorCoroutineUtility.StartCoroutineOwnerless(LoadSavedLanguage(removedLanguage.Item2));
        //     LoadSavedLanguage(removedLanguage.Item2);
        //     
        //     // TODO: Check before create if there is a language with name already created
        //     UpdateLanguageDisplayOrder(removedLanguage.Item2.Language, localizationData.languages.Count, removedLanguage.Item1);
        //     
        //     // foreach (var languageData in localizationData.languages.Where(x=> x.displayOrder >= removedLanguage.Item1))
        //     // {
        //     //     languageData.displayOrder = Math.Clamp(languageData.displayOrder+1, 0, localizationData.languages.Count+1);
        //     // }
        //     
        //     UpdateLanguageCache();
        //     LocalizationMainEditor.Instance.Repaint();
        //     SaveChanges();
        // }

        public static void UpdateLanguageDisplayOrder(string languageName, int oldDisplayOrder, int newDisplayOrder)
        {
            // Find the category to update
            var languageToUpdate = localizationData.LanguagesDictionary[languageName];
            
            Undo.RecordObject(languageToUpdate, "Changed language display order");

            // Reorder categories
            if (oldDisplayOrder < newDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var language in localizationData.languages.Where(language => language.displayOrder > oldDisplayOrder && language.displayOrder <= newDisplayOrder))
                {
                    Undo.RecordObject(language, "Changed language display order");
                    language.displayOrder--; // Shift down
                }
            }
            else if (oldDisplayOrder > newDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var language in localizationData.languages.Where(language => language.displayOrder >= newDisplayOrder && language.displayOrder < oldDisplayOrder))
                {
                    Undo.RecordObject(language, "Changed language display order");
                    language.displayOrder++; // Shift up
                }
            }
            
            // Update the target's display order
            languageToUpdate!.displayOrder = newDisplayOrder;

            UpdateLanguageCache();
            SaveChanges();

            Undo.undoRedoPerformed -= UpdateLanguagesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateLanguagesCacheAndRepaint;
        }

        private static void UpdateLanguagesCacheAndRepaint()
        {
            UpdateLanguageCache();
            LocalizationMainEditor.Instance.Repaint();
            SaveChanges();
        }

        public static void UpdateLanguageName(string oldLanguageName, string newLanguageName)
        {
            var languageToUpdate = localizationData.LanguagesDictionary[oldLanguageName];
            
            Undo.RecordObject(languageToUpdate, $"Change Language {oldLanguageName} Name to {newLanguageName}");
            UndoRenameLanguageTracker.Push((oldLanguageName, newLanguageName));
            
            languageToUpdate!.languageName = newLanguageName;
            EditorUtility.SetDirty(languageToUpdate);
            AssetDatabase.RenameAsset(GetPath(languageToUpdate), newLanguageName);

            SaveChanges();
            
            Undo.undoRedoPerformed -= HandleUndoRedoLanguageName;
            Undo.undoRedoPerformed += HandleUndoRedoLanguageName;
        }
        
        private static void HandleUndoRedoLanguageName()
        {
            if (UndoRenameLanguageTracker.IsEmpty()) return;
            var previousCurrentName = UndoRenameLanguageTracker.Pop();
            
            var assetPath = $"{LANGUAGES_PATH}/{previousCurrentName.Item2}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath).IsNull()) return;
            AssetDatabase.RenameAsset(assetPath, previousCurrentName.Item1);
            
            SaveChanges();
        }

        public static void EmptyLanguage(string languageToEmpty)
        {
            var languageToUpdate = localizationData.LanguagesDictionary[languageToEmpty];
            foreach (var translationKeyData in languageToUpdate.translationKeys)
            {
                Undo.RecordObject(translationKeyData, $"Empty language {languageToEmpty}");
                translationKeyData.translationText = "";
            }

            SaveChanges();
        }

        #endregion

        #region CATEGORIES

        public static void InsertCategory(string newCategoryName)
        {
            var newCategory = ScriptableObject.CreateInstance<CategoryDataSO>();
            newCategory.categoryName = newCategoryName;
            newCategory.displayOrder = localizationData.categories.Count + 1;
            if (localizationData.categories.Count == 0) newCategory.isDefault = true;
            AssetDatabase.CreateAsset(newCategory, $"{CATEGORIES_PATH}/{newCategoryName}.asset");

            localizationData.categories.Add(newCategory);
            SaveChanges(newCategory);
            UpdateCategoriesCache();
        }

        public static void SetDefaultCategory(string newDefaultCategory)
        {
            // Remove old default
            var currentDefault = CategoryCache[0];
            Undo.RecordObject(currentDefault, $"Change default category to {newDefaultCategory}");
            currentDefault.isDefault = false;

            // Set new default
            var newDefault = localizationData.CategoriesDictionary[newDefaultCategory];
            Undo.RecordObject(newDefault, $"Change default category to {newDefaultCategory}");
            newDefault.isDefault = true;

            // Reorder categories
            var newDefaultOrder = newDefault.displayOrder;
            foreach (var category in localizationData.categories.Where(category => category.displayOrder < newDefaultOrder))
            {
                Undo.RecordObject(category, $"Change default category to {newDefaultCategory}");
                category.displayOrder++; // Shift categories above the new default
            }
            
            
            // Update keys
            var affectedKeys = localizationData.keys.Where(x => x.category.categoryName.Equals(currentDefault.categoryName)).ToList();
            foreach (var key in affectedKeys)
            {
                Undo.RecordObject(key, $"Change default category to {newDefaultCategory}");
                key.category = newDefault;
            }
            
            newDefault.displayOrder = 1;

            UpdateCategoriesCache();
            SaveChanges();
            
            Undo.undoRedoPerformed -= UpdateCategoriesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateCategoriesCacheAndRepaint;
        }
        
        public static void RemoveCategory(string categoryName)
        {
            var categoryToRemove = localizationData.CategoriesDictionary[categoryName];
            var removedDisplayOrder = categoryToRemove!.displayOrder;
            
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Remove category {categoryToRemove.categoryName}");

            var affectedKeys = localizationData.keys.Where(x => x.category.categoryName.Equals(categoryName)).ToList();
            
            UndoRemoveCategoryTracker.Push((Object.Instantiate(categoryToRemove), affectedKeys));
            
            // Update keys that use the categoryToRemove to default category
            foreach (var key in affectedKeys)
            {
                key.category = localizationData.CategoriesDictionary[DefaultCategory];
            }

            // Update display order
            foreach (var category in localizationData.categories.Where(category => category.displayOrder > removedDisplayOrder))
            {
                category.displayOrder--;
            }

            localizationData.categories.Remove(categoryToRemove);

            RemoveAsset(categoryToRemove);

            Undo.CollapseUndoOperations(undoGroup);
            
            UpdateCategoriesCache();
            SaveChanges();
            
            Undo.undoRedoPerformed -= HandleUndoRedoRemovedCategory;
            Undo.undoRedoPerformed += HandleUndoRedoRemovedCategory;
        }
        
        private static void HandleUndoRedoRemovedCategory()
        {
            if (UndoRemoveCategoryTracker.IsEmpty()) return;
            var removedCategory = UndoRemoveCategoryTracker.Pop();
            
            // TODO: Check before create if there is a category with name already created
            
            AssetDatabase.CreateAsset(removedCategory.Item1, $"{CATEGORIES_PATH}/{removedCategory.Item1.categoryName}.asset");
            foreach (var categoryData in localizationData.categories.Where(x=> x.displayOrder >= removedCategory.Item1.displayOrder))
            {
                categoryData.displayOrder = Math.Clamp(categoryData.displayOrder+1, 0, localizationData.categories.Count+1);
            }
            
            localizationData.categories.Add(removedCategory.Item1);
            
            foreach (var keyData in removedCategory.Item2)
            {
                keyData.category = removedCategory.Item1;
            }
            
            UpdateCategoriesCache();
            LocalizationMainEditor.Instance.Repaint();
            SaveChanges();
        }

        public static void UpdateCategoryDisplayOrder(string categoryName, int oldCategoryDisplayOrder, int newCategoryDisplayOrder)
        {
            // Find the category to update
            var categoryToUpdate = localizationData.CategoriesDictionary[categoryName];

            Undo.RecordObject(categoryToUpdate, "Changed category display order");
            
            // Reorder categories
            if (oldCategoryDisplayOrder < newCategoryDisplayOrder)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var category in localizationData.categories.Where(category => category.displayOrder > oldCategoryDisplayOrder && category.displayOrder <= newCategoryDisplayOrder))
                {
                    Undo.RecordObject(category, "Changed category display order");
                    category.displayOrder--; // Shift categories down
                }
            }
            else if (oldCategoryDisplayOrder > newCategoryDisplayOrder)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var category in localizationData.categories.Where(category => category.displayOrder >= newCategoryDisplayOrder && category.displayOrder < oldCategoryDisplayOrder))
                {
                    Undo.RecordObject(category, "Changed category display order");
                    category.displayOrder++; // Shift categories up
                }
            }

            // Update the target category's display order
            categoryToUpdate!.displayOrder = newCategoryDisplayOrder;

            UpdateCategoriesCache();
            SaveChanges();
            
            Undo.undoRedoPerformed -= UpdateCategoriesCacheAndRepaint;
            Undo.undoRedoPerformed += UpdateCategoriesCacheAndRepaint;
        }

        private static void UpdateCategoriesCacheAndRepaint()
        {
            UpdateCategoriesCache();
            LocalizationMainEditor.Instance.Repaint();
            SaveChanges();
        }
        
        public static void UpdateCategoryName(string oldCategoryName, string newCategoryName)
        {
            var categoryToUpdate = localizationData.CategoriesDictionary[oldCategoryName];
            
            Undo.RecordObject(categoryToUpdate, $"Change Category {oldCategoryName} Name to {newCategoryName}");
            UndoRenameCategoryTracker.Push((oldCategoryName, newCategoryName));
            
            categoryToUpdate!.categoryName = newCategoryName;
            EditorUtility.SetDirty(categoryToUpdate);
            AssetDatabase.RenameAsset(GetPath(categoryToUpdate), newCategoryName);
            
            SaveChanges();
            
            Undo.undoRedoPerformed -= HandleUndoRedoCategoryName;
            Undo.undoRedoPerformed += HandleUndoRedoCategoryName;
        }
        
        private static void HandleUndoRedoCategoryName()
        {
            if (UndoRenameCategoryTracker.IsEmpty()) return;
            var previousCurrentName = UndoRenameCategoryTracker.Pop();
            
            var assetPath = $"{CATEGORIES_PATH}/{previousCurrentName.Item2}.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath).IsNull()) return;
            AssetDatabase.RenameAsset(assetPath, previousCurrentName.Item1);
            
            SaveChanges();
        }

        #endregion

        #region PRIVATE METHODS

        private static void CreateOrLoadLocalizationData()
        {
            CreateDirectoryIfNotExist(DATABASE_PATH);

            localizationData = AssetDatabase.LoadAssetAtPath<LocalizationDataSO>(LOCALIZATION_DATA_PATH) ?? ScriptableObject.CreateInstance<LocalizationDataSO>();

            if (!AssetDatabase.Contains(localizationData))
            {
                AssetDatabase.CreateAsset(localizationData, LOCALIZATION_DATA_PATH);
                //Debug.Log("LocalizationData.asset created!");
            }

            InsertFirstLanguageIfNotExist();
            InsertFirstCategoryIfNotExist();
            SaveChanges();

            //Debug.Log("LocalizationData.asset loaded!");
        }

        private static void InsertFirstLanguageIfNotExist()
        {
            if (localizationData.languages.IsNotEmpty()) return;

            CreateDirectoryIfNotExist(LANGUAGES_PATH);
            InsertLanguage("English");
        }

        private static void InsertFirstCategoryIfNotExist()
        {
            if (localizationData.categories.IsNotEmpty()) return;

            CreateDirectoryIfNotExist(CATEGORIES_PATH);
            InsertCategory("None");
        }

        private static void SaveChanges(Object obj = null)
        {
            EditorUtility.SetDirty(localizationData);
            if (obj.IsNotNull()) EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static void RemoveAsset(Object assetToRemove)
        {
            var assetPath = GetPath(assetToRemove);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static void CreateDirectoryIfNotExist(string path)
        {
            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }


        private static string GetPath(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset);
        }

        private static void UpdateLanguageCache()
        {
            LanguageCache = localizationData.languages
                .OrderBy(l => l.displayOrder)
                .ToList();
        }

        private static void UpdateCategoriesCache()
        {
            CategoryCache = localizationData.categories
                .OrderBy(c => c.displayOrder)
                .ToList();
        }

        #endregion
    }
}
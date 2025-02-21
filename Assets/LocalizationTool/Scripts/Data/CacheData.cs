using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CustomDebugPlugin;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using Mono.Data.Sqlite;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorPaths;
using static LocalizationTool.Database.DatabaseStrings;
using Colors = CustomDebugPlugin.Colors;

namespace LocalizationTool.Scripts.Data
{
    public class CacheData
    {
        #region PUBLIC VARIABLES

        public static CacheData Instance => _instance ??= new CacheData();

        public static bool IsDataLoaded { get; private set; }
        
        //public static Dictionary<string, KeyData> DictionaryCache { get; set; }
        //public static List<string> Keys => DictionaryCache.Select(x => x.Key).ToList();

        #region Actions

        public Action OnLocalizationToolDataInitialized { get; set; }

        #endregion

        #endregion

        #region PRIVATE VARIABLES

        private static CacheData _instance;
        private static string _connectionString;
        private static IDbConnection _dbConnection;
        private bool _isInitialized;

        #endregion

        #region LOAD CACHE

        public void InitForEditor(Action onComplete = null)
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            LoadCacheData();

            onComplete?.Invoke();
        }

        public static void LoadCacheData()
        {
            IsDataLoaded = true;
            CustomDebug.Log("Database", Colors.Red, "Loaded");
            Instance.OnLocalizationToolDataInitialized?.Invoke();
        }
        
        #endregion

        #region DATABASE UPDATES

        public static void OpenDatabaseConnection()
        {
            _connectionString = "URI=file:" + DATABASE_PATH;
            _dbConnection = new SqliteConnection(_connectionString);
            _dbConnection.Open();
        }

        public static void CloseDatabaseConnection()
        {
            if (_dbConnection == null) return;

            _dbConnection.Close();
            _dbConnection = null;
        }

        public static void ExecuteNonQueryCommand(string query)
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public static IDataReader ExecuteReaderCommand(string query)
        {
            if (_dbConnection.IsNull()) OpenDatabaseConnection();
            using var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteReader();
        }

        private static void DeleteDatabase()
        {
            try
            {
                OpenDatabaseConnection();
                //Delete translation
                var query = $"DELETE FROM {TRANSLATION_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_TABLE}';";
                ExecuteNonQueryCommand(query);

                //Delete translations_keys
                query = $"DELETE FROM {TRANSLATION_KEY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_KEY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete category
                query = $"DELETE FROM {CATEGORY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{CATEGORY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete language
                query = $"DELETE FROM {LANGUAGE_TABLE}; DELETE FROM sqlite_sequence WHERE name='{LANGUAGE_TABLE}';";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }
        
        #region UPDATE CACHES

        public static void ClearData()
        {
            DeleteDatabase();
        }
        
        #endregion
        
        #endregion
    }
}
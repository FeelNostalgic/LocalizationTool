using UnityEngine;
using UnityEditor;
using System.Data;
using Mono.Data.Sqlite;

namespace LocalizationTool.Scripts.Test
{
    public class SqliteEditor : EditorWindow
    {
        private string connectionString;
        private IDbConnection dbConnection;
        private string dbFilePath = "Assets/Database/myDatabase.db";

        [MenuItem("Tools/Sqlite", false, -30)]
        public static void ShowWindow()
        {
            var window = GetWindow<SqliteEditor>("SQLITE");
            window.minSize = new Vector2(600, 500);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Create Table"))
            {
                CreateTable();
            }

            if (GUILayout.Button("Insert Data"))
            {
                InsertData();
            }

            if (GUILayout.Button("Read Data"))
            {
                ReadData();
            }
        }

        private void OnEnable()
        {
            connectionString = "URI=file:" + dbFilePath;
            OpenConnection();
        }

        private void OnDisable()
        {
            CloseConnection();
        }
        
        private void OpenConnection()
        {
            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();
            Debug.Log("Database connected");
        }

        private void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection = null;
                Debug.Log("Database connection closed");
            }
        }

        private void CreateTable()
        {
            using var dbCommand = dbConnection.CreateCommand();
            string query = "CREATE TABLE IF NOT EXISTS my_table (id INTEGER PRIMARY KEY, language TEXT, value TEXT)";
            dbCommand.CommandText = query;
            dbCommand.ExecuteNonQuery();
            Debug.Log("Table created");
        }
        
        private void InsertData()
        {
            using var dbCommand = dbConnection.CreateCommand();
            string query = "INSERT INTO my_table (language, value) VALUES ('English', 'Ok')";
            dbCommand.CommandText = query;
            dbCommand.ExecuteNonQuery();
            Debug.Log("Data inserted");
        }
        
        private void ReadData()
        {
            using var dbCommand = dbConnection.CreateCommand();
            string query = "SELECT * FROM my_table";
            dbCommand.CommandText = query;

            using IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                Debug.Log($"id {reader["id"]}, language: {reader["language"]}, value: {reader["value"]}");
            }
        }
    }
}
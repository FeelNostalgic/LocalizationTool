
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using LocalizationTool.Scripts.Serializer;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Scripts.Data
{ 
	public abstract class SaveLoadFileManager 
	{
        public static async Task SaveFile<T>(T fileToSave, string path, ISerializerService serializer)
        {
            var dataToSave = serializer.Serialize(fileToSave);
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path);
                await writer.WriteAsync(dataToSave);
            }
            catch (Exception e)
            {
                if (e is not IOException) Debug.LogError($"PATH: {path} => {e}");
            }
            finally
            {
                writer?.Close();
            }
        }
        
        public static async void SaveFile(string path, string fileContent)
        {
            await File.WriteAllTextAsync(path, fileContent);
        }
        
        public static async void SaveFile(string path, string fileContent, string title, string message, string okMessage)
        {
            await File.WriteAllTextAsync(path, fileContent);

            EditorUtility.DisplayDialog(title, message, okMessage);
        }

        public static async Task<T> LoadFile<T>(string path, ISerializerService serializer)
        {
            StreamReader reader = null;

            try
            {
                var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
                reader = new StreamReader(fs);
                return serializer.Deserialize<T>(await reader.ReadToEndAsync());
            }
            catch (Exception e)
            {
                if (e is SerializationException)
                {
                    Debug.LogError($"PATH: {path} => {e}");
                }
                else
                {
                    Debug.LogError($"PATH: {path} => {e}");
                }
            }
            finally
            {
                reader?.Close();
            }

            return default;
        }
	}
}
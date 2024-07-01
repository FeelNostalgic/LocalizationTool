using UnityEngine;

namespace CodingTest.Serializer
{
    public class UnityJsonSerializer : ISerializerService
    {
        public string Extension => ".json";
        public string Serialize<T>(T data)
        {
            return JsonUtility.ToJson(data);
        }

        public T Deserialize<T>(string data)
        {
            return JsonUtility.FromJson<T>(data);
        }
    }
}
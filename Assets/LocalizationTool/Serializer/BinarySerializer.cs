using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CodingTest.Serializer
{
    public class BinarySerializer : ISerializerService
    {
        private readonly BinaryFormatter _formatter;
        
        public BinarySerializer()
        {
            _formatter = new BinaryFormatter();
        }
        
        public string Extension => ".bin";
        
        public string Serialize<T>(T data)
        {
            using MemoryStream memoryStream = new MemoryStream();
            _formatter.Serialize(memoryStream, data);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public T Deserialize<T>(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return (T) _formatter.Deserialize(memoryStream);
        }
    }
}
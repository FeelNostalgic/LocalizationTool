namespace CodingTest.Serializer
{
    public interface ISerializerService
    {
        public string Extension { get; }
        public string Serialize<T>(T data);
        public T Deserialize<T>(string data);
    }
}
using System.IO;
using System.Xml.Serialization;

namespace LocalizationTool.Scripts.Serializer
{ 
	public class XmlSerializerService : ISerializerService
	{
		public string Extension => ".xml";
		public string Serialize<T>(T data)
		{
			var serializer = new XmlSerializer(typeof(T));
			using var stringWriter = new StringWriter();
			
			serializer.Serialize(stringWriter, data);
			return stringWriter.ToString();
		}

		public T Deserialize<T>(string data)
		{
			var serializer = new XmlSerializer(typeof(T));
			using var stringReader = new StringReader(data);
			return (T)serializer.Deserialize(stringReader);
		}
	}
}
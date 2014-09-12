using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Vindinium.Common
{
	public static class JsonHelper
	{
		public static T JsonToObject<T>(this string json) where T : class
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using (var stream = new MemoryStream(byteArray))
			{
				var serializer = new DataContractJsonSerializer(typeof (T));
				return serializer.ReadObject(stream) as T;
			}
		}

		public static string ToJson<T>(this T graph) where T : class
		{
			using (var stream = new MemoryStream())
			{
				var serializer = new DataContractJsonSerializer(typeof (T));
				serializer.WriteObject(stream, graph);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
	}
}
namespace Vindinium.Common.Services
{
	public interface IJsonDeserializer
	{
		T Deserialize<T>(string json) where T : class;
	}
}
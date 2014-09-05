namespace Vindinium.Common.Entities
{
	public interface IApiRequest
	{
		string Url { get; }
		string Parameters { get; }
	}
}
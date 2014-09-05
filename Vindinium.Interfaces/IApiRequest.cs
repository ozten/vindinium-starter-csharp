namespace Vindinium.Interfaces
{
	public interface IApiRequest
	{
		string Url { get; }
		string Parameters { get; }
	}
}
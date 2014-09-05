namespace Vindinium.Interfaces
{
	public interface IApiResponse
	{
		string ErrorMessage { get; }
		string Text { get; }
		bool HasError { get; }
	}
}
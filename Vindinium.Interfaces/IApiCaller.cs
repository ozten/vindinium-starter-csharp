namespace Vindinium.Interfaces
{
	public interface IApiCaller
	{
		IApiResponse Get(IApiRequest apiRequest);
	}
}
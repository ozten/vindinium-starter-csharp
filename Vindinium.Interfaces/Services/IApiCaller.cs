using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
	public interface IApiCaller
	{
		IApiResponse Get(IApiRequest apiRequest);
	}
}
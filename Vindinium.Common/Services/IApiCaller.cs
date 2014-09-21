using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IApiCaller
    {
        IApiResponse Call(IApiRequest apiRequest);
    }
}
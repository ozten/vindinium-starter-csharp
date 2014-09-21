using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IApiEndpointBuilder
    {
        IApiRequest StartArena();
        IApiRequest StartTraining(uint turns);
        IApiRequest Play(string gameId, string token, Direction direction);
    }
}
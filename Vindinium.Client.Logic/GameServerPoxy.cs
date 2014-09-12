using Vindinium.Common;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class GameServerPoxy : IGameServerPoxy
    {
        private readonly IApiCaller _caller;
        private readonly IApiEndpointBuilder _endpointBuilder;

        public GameServerPoxy(IApiCaller caller, IApiEndpointBuilder endpointBuilder)
        {
            _caller = caller;
            _endpointBuilder = endpointBuilder;
        }

        public string StartTraining(string userId, uint turns)
        {
            return _caller.Call(_endpointBuilder.StartTraining(turns)).Text;
        }

        public string StartArena(string userId)
        {
            return _caller.Call(_endpointBuilder.StartArena()).Text;
        }

        public string Play(string gameId, string token, Direction direction)
        {
            return _caller.Call(_endpointBuilder.Play(gameId, token, direction)).Text;
        }
    }
}
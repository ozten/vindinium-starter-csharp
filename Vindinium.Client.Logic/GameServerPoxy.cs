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

        public IApiResponse StartTraining(string userId, uint turns)
        {
            return _caller.Call(_endpointBuilder.StartTraining(turns));
        }

        public IApiResponse StartArena(string userId)
        {
            return _caller.Call(_endpointBuilder.StartArena());
        }

        public IApiResponse Play(string gameId, string token, Direction direction)
        {
            return _caller.Call(_endpointBuilder.Play(gameId, token, direction));
        }
    }
}
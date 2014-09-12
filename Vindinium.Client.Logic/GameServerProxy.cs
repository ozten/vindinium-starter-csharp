using System;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class GameServerProxy : IGameServerProxy
    {
        private readonly IApiCaller _caller;
        private readonly IApiEndpointBuilder _endpointBuilder;

        public GameServerProxy(IApiCaller caller, IApiEndpointBuilder endpointBuilder)
        {
            _caller = caller;
            _endpointBuilder = endpointBuilder;
        }

        public GameResponse GameResponse { get; private set; }
        public IApiResponse ApiResponse { get; private set; }

        public string StartTraining(uint turns)
        {
            return CallApi(_endpointBuilder.StartTraining(turns));
        }

        public string StartArena()
        {
            return CallApi(_endpointBuilder.StartArena());
        }

        public string Play(string gameId, string token, Direction direction)
        {
            return CallApi(_endpointBuilder.Play(gameId, token, direction));
        }


        public string Start(string mapText)
        {
            throw new NotImplementedException();
        }

        private string CallApi(IApiRequest request)
        {
            ApiResponse = _caller.Call(request);
            if (ApiResponse.HasError)
            {
                GameResponse = null;
                return ApiResponse.ErrorMessage;
            }
            GameResponse = ApiResponse.Text.JsonToObject<GameResponse>();
            return ApiResponse.Text;
        }
    }
}
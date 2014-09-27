using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class GameServerProxy : IGameServerProxy
    {
        private readonly IApiResponse _apiResponse;
        private readonly IApiCaller _caller;
        private readonly IApiEndpointBuilder _endpointBuilder;

        public GameServerProxy(IApiCaller caller, IApiEndpointBuilder endpointBuilder, IApiResponse apiResponse)
        {
            _caller = caller;
            _endpointBuilder = endpointBuilder;
            _apiResponse = apiResponse;
        }

        public GameResponse GameResponse { get; private set; }

        public IApiResponse StartTraining(uint turns)
        {
            CallApi(_endpointBuilder.StartTraining(turns), _apiResponse);
            return _apiResponse;
        }

        public IApiResponse StartArena()
        {
            CallApi(_endpointBuilder.StartArena(), _apiResponse);
            return _apiResponse;
        }

        public IApiResponse Play(string gameId, string token, Direction direction)
        {
            CallApi(_endpointBuilder.Play(gameId, token, direction), _apiResponse);
            return _apiResponse;
        }

        private void CallApi(IApiRequest request, IApiResponse response)
        {
            _caller.Call(request, response);
            if (response.HasError)
            {
                GameResponse = null;
            }
            GameResponse = response.Text.JsonToObject<GameResponse>();
        }
    }
}
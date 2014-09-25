using System;
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

        public IApiResponse ApiResponse
        {
            get { return _apiResponse; }
        }

        public GameResponse GameResponse { get; private set; }

        public string StartTraining(uint turns)
        {
            return CallApi(_endpointBuilder.StartTraining(turns), _apiResponse);
        }

        public string StartArena()
        {
            return CallApi(_endpointBuilder.StartArena(), _apiResponse);
        }

        public string Play(string gameId, string token, Direction direction)
        {
            return CallApi(_endpointBuilder.Play(gameId, token, direction), _apiResponse);
        }


        public void ChangeMap(string mapText)
        {
            throw new NotImplementedException();
        }

        public string Start(string mapText)
        {
            throw new NotImplementedException();
        }

        private string CallApi(IApiRequest request, IApiResponse response)
        {
            _caller.Call(request, response);
            if (response.HasError)
            {
                GameResponse = null;
                return response.ErrorMessage;
            }
            GameResponse = response.Text.JsonToObject<GameResponse>();
            return response.Text;
        }
    }
}
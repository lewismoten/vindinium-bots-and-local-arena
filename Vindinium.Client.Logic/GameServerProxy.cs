using Vindinium.Common;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class GameServerProxy : IGameServerProxy
    {
        private readonly IApiCaller _caller;
        private readonly IApiEndpointBuilder _endpointBuilder;

        public GameServerProxy(IApiCaller caller, IApiEndpointBuilder endpointBuilder, IApiResponse apiResponse)
        {
            _caller = caller;
            _endpointBuilder = endpointBuilder;
            Response = apiResponse;
        }

        public IApiResponse Response { get; private set; }

        public void StartTraining(uint turns)
        {
            CallApi(_endpointBuilder.StartTraining(turns), Response);
        }

        public void StartArena()
        {
            CallApi(_endpointBuilder.StartArena(), Response);
        }

        public void Play(string gameId, string token, Direction direction)
        {
            CallApi(_endpointBuilder.Play(gameId, token, direction), Response);
        }

        private void CallApi(IApiRequest request, IApiResponse response)
        {
            _caller.Call(request, response);
        }
    }
}
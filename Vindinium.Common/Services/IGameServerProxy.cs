using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IGameServerProxy
    {
        IApiResponse Response { get; }
        void StartTraining(uint rounds);
        void StartArena();
        void Play(string gameId, string token, Direction direction);
    }
}
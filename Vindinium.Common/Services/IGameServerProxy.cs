using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IGameServerProxy
    {
        IApiResponse Response { get; }
        void StartTraining(uint turns);
        void StartArena();
        void Play(string gameId, string token, Direction direction);
    }
}
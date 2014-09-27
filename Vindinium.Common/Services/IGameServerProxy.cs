using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IGameServerProxy
    {
        GameResponse GameResponse { get; }
        IApiResponse StartTraining(uint turns);
        IApiResponse StartArena();
        IApiResponse Play(string gameId, string token, Direction direction);
    }
}
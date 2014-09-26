using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public interface IGameStateProvider
    {
        GameResponse Game { get; set; }
    }
}
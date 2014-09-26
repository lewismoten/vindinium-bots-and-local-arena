using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockGameStateProvider : IGameStateProvider
    {
        public GameResponse Game { get; set; }
    }
}
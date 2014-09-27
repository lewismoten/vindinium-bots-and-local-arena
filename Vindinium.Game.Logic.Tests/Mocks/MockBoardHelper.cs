using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockBoardHelper : BoardHelper
    {
        public static Board Board = new Board();

        public MockBoardHelper() : base(Board)
        {
        }
    }
}
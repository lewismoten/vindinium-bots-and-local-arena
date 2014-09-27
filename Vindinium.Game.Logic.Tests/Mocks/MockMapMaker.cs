namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockMapMaker : IMapMaker
    {
        public string MapText { get; set; }

        public string GenerateMap(int seed, IBoardHelper boardHelper)
        {
            return MapText;
        }
    }
}
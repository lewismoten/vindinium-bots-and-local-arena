namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockMapMaker : IMapMaker
    {
        public string MapText { get; set; }

        public void GenerateMap(IBoardHelper boardHelper)
        {
            boardHelper.MapText = MapText;
        }
    }
}
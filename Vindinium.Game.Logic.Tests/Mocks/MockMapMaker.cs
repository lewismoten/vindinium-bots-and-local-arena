namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockMapMaker : IMapMaker
    {
        public string GenerateMap(int seed)
        {
            return "@1@2@3@4";
        }
    }
}
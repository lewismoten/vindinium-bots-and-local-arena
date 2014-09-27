using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Services;
using Vindinium.Game.Logic.Tests.Mocks;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerStartArenaTests
    {
        [TestFixtureSetUp]
        public void RunBeforeFirstTest()
        {
            var mockMapMaker = new MockMapMaker {MapText = "@1@2@3@4"};
            var mockResponse = new MockApiResponse();
            IGameServerProxy server = new GameServer(mockMapMaker, mockResponse, new MockGameStateProvider());
            server.StartArena();
            _gameResponse = mockResponse.Text.JsonToObject<GameResponse>();
            _game = _gameResponse.Game;
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;

        [Test]
        public void AllPlayersHaveEloScore()
        {
            Assert.That(_game.Players, Has.All.Property("Elo").InRange(0, 3000));
        }
    }
}
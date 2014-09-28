using NSubstitute;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
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
            var mockMapMaker = Substitute.For<IMapMaker>();
            var apiResponse = Substitute.For<IApiResponse>();
            var boardHelper = Substitute.For<IBoardHelper>();
            boardHelper.MapText = "@1@2@3@4";
            IGameServerProxy server = new GameServer(mockMapMaker, apiResponse, new MockGameStateProvider(),
                boardHelper);
            server.StartArena();
            _gameResponse = apiResponse.Text.JsonToObject<GameResponse>();
            _game = _gameResponse.Game;
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;

        [Test]
        public void AllPlayersHaveEloScore()
        {
            Assert.That(_game.Players, Has.All.Property("Elo").InRange(0, 3000));
        }


        [Test]
        public void MaxTurnsAre1200()
        {
            Assert.That(_game.MaxTurns, Is.EqualTo(1200));
        }
    }
}
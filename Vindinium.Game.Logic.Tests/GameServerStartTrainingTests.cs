using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Services;
using Vindinium.Game.Logic.Tests.Mocks;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerStartTrainingTests
    {
        [TestFixtureSetUp]
        public void RunBeforeFirstTest()
        {
            var mockMapMaker = new MockMapMaker {MapText = "@1@2@3@4"};
            var mockApiResponse = new MockApiResponse();
            IGameServerProxy server = new GameServer(mockMapMaker, mockApiResponse, new MockGameStateProvider(),
                new BoardHelper());
            server.StartTraining(300);
            _gameResponse = mockApiResponse.Text.JsonToObject<GameResponse>();
            _game = _gameResponse.Game;
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;

        [Test]
        public void MaxTurnsEqualRoundsTimesFour()
        {
            const uint rounds = 23;
            var mockMapMaker = new MockMapMaker {MapText = "@1@2@3@4"};
            var mockApiResponse = new MockApiResponse();
            IGameServerProxy server = new GameServer(mockMapMaker, mockApiResponse, new MockGameStateProvider(),
                new BoardHelper());
            server.StartTraining(rounds);
            _gameResponse = mockApiResponse.Text.JsonToObject<GameResponse>();
            Assert.That(_gameResponse.Game.MaxTurns, Is.EqualTo(rounds*4));
        }

        [Test]
        public void OnlySelfHasEloScoreInTraining()
        {
            Assert.That(_gameResponse.Self.Elo, Is.Not.Null);
            Assert.That(_game.Players.Except(new[] {_gameResponse.Self}), Has.All.Property("Elo").Null);
        }
    }
}
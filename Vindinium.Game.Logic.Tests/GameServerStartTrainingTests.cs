using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerStartTrainingTests
    {
        [TestFixtureSetUp]
        public void RunBeforeFirstTest()
        {
            var mapMaker = Substitute.For<IMapMaker>();
            var boardHelper = Substitute.For<IBoardHelper>();
            boardHelper.MapText = "@1@2@3@4";
            var apiResponse = Substitute.For<IApiResponse>();
            IGameServerProxy server = new GameServer(mapMaker, apiResponse, Substitute.For<IGameStateProvider>(),
                boardHelper);
            server.StartTraining(300);
            _gameResponse = apiResponse.Text.JsonToObject<GameResponse>();
            _game = _gameResponse.Game;
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;

        [Test]
        public void MaxTurnsEqualRoundsTimesFour()
        {
            const uint rounds = 23;
            var mapMaker = Substitute.For<IMapMaker>();
            var boardHelper = Substitute.For<IBoardHelper>();
            boardHelper.MapText = "@1@2@3@4";
            var apiResponse = Substitute.For<IApiResponse>();
            IGameServerProxy server = new GameServer(mapMaker, apiResponse, Substitute.For<IGameStateProvider>(), boardHelper);
            server.StartTraining(rounds);
            _gameResponse = apiResponse.Text.JsonToObject<GameResponse>();
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
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;
using Vindinium.Game.Logic.Tests.Mocks;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerEndGameTests
    {
        private IGameServerProxy _server;
        private readonly IApiResponse _apiResponse = new MockApiResponse();
        private MockMapMaker _mockMapMaker;
        private MockGameStateProvider _mockGameStateProvider;

        [Test]
        public void PlayAfterCrashReturnsError()
        {
            _mockMapMaker = new MockMapMaker();
            _mockGameStateProvider = new MockGameStateProvider();
            _server = new GameServer(_mockMapMaker, _apiResponse, _mockGameStateProvider, new BoardHelper());
            _mockGameStateProvider.Game = new GameResponse
            {
                Self = new Hero {Crashed = true},
                Game = new Common.DataStructures.Game()
            };

            _server.Play(_mockGameStateProvider.Game.Game.Id,
                _mockGameStateProvider.Game.Token,
                Direction.Stay);

            Assert.That(_apiResponse.Text, Is.Null);
            Assert.That(_apiResponse.HasError, Is.True);
            Assert.That(_apiResponse.ErrorMessage, Is.EqualTo("You have crashed and can no longer play"));
        }

        [Test]
        public void PlayAfterFinishedReturnsError()
        {
            _mockMapMaker = new MockMapMaker();
            _mockGameStateProvider = new MockGameStateProvider();
            _server = new GameServer(_mockMapMaker, _apiResponse, _mockGameStateProvider, new BoardHelper());
            _mockGameStateProvider.Game = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Finished = true
                }
            };

            _server.Play(_mockGameStateProvider.Game.Game.Id, _mockGameStateProvider.Game.Token,
                Direction.Stay);

            Assert.That(_apiResponse.Text, Is.Null);
            Assert.That(_apiResponse.HasError, Is.True);
            Assert.That(_apiResponse.ErrorMessage, Is.EqualTo("Game has finished"));
        }

        [Test]
        public void PlayLastTurnReturnsFinished()
        {
            _mockGameStateProvider = new MockGameStateProvider();
            _server = new GameServer(new MapMaker(new Randomizer()), _apiResponse, _mockGameStateProvider,
                new BoardHelper());
            _server.StartTraining(1);
            _mockGameStateProvider.Game.Game.Turn = 4;

            _server.Play(_mockGameStateProvider.Game.Game.Id, _mockGameStateProvider.Game.Token,
                Direction.Stay);

            var response = _apiResponse.Text.JsonToObject<GameResponse>();
            Assert.That(response.Game.Finished, Is.True);
        }
    }
}
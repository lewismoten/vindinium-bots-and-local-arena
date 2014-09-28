using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerEndGameTests
    {
        private static IApiResponse PlayDirection(IGameStateProvider gameStateProvider)
        {
            var apiResponse = Substitute.For<IApiResponse>();
            var mapMaker = Substitute.For<IMapMaker>();
            var boardHelper = Substitute.For<IBoardHelper>();

            var server = new GameServer(mapMaker, apiResponse, gameStateProvider, boardHelper);

            GameResponse response = gameStateProvider.Game;
            server.Play(response.Game.Id, response.Token, Direction.Stay);

            return apiResponse;
        }

        [Test]
        public void PlayAfterCrashReturnsError()
        {
            var gameStateProvider = Substitute.For<IGameStateProvider>();

            gameStateProvider.Game = new GameResponse
            {
                Self = new Hero {Crashed = true},
                Game = new Common.DataStructures.Game()
            };

            IApiResponse apiResponse = PlayDirection(gameStateProvider);

            Assert.That(apiResponse.Text, Is.Null);
            Assert.That(apiResponse.HasError, Is.True);
            Assert.That(apiResponse.ErrorMessage, Is.EqualTo("You have crashed and can no longer play"));
        }

        [Test]
        public void PlayAfterFinishedReturnsError()
        {
            var gameStateProvider = Substitute.For<IGameStateProvider>();

            gameStateProvider.Game = new GameResponse
            {
                Game = new Common.DataStructures.Game {Finished = true}
            };

            IApiResponse apiResponse = PlayDirection(gameStateProvider);

            Assert.That(apiResponse.Text, Is.Null);
            Assert.That(apiResponse.HasError, Is.True);
            Assert.That(apiResponse.ErrorMessage, Is.EqualTo("Game has finished"));
        }

        [Test]
        public void PlayLastTurnReturnsFinished()
        {
            var gameStateProvider = Substitute.For<IGameStateProvider>();
            gameStateProvider.Game = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = new Board
                    {
                        MapText = "@1      "
                    },
                    Players = new List<Hero>
                    {
                        new Hero {Id = 1, Pos = new Pos {X = 1, Y = 1}}
                    }
                },
                Self = new Hero {Id = 1}
            };

            IApiResponse apiResponse = PlayDirection(gameStateProvider);

            var response2 = apiResponse.Text.JsonToObject<GameResponse>();
            Assert.That(response2.Game.Finished, Is.True);
        }
    }
}
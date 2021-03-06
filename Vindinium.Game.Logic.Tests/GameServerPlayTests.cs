﻿using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;
using Vindinium.Game.Logic.Tests.Mocks;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerPlayTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
            _mockMapMaker = new MockMapMaker();
            _mockGameStateProvider = new MockGameStateProvider();
            _server = new GameServer(_mockMapMaker, _apiResponse, _mockGameStateProvider, new BoardHelper());
        }

        private IGameServerProxy _server;
        private readonly IApiResponse _apiResponse = new MockApiResponse();
        private MockMapMaker _mockMapMaker;
        private MockGameStateProvider _mockGameStateProvider;

        private GameResponse Play(string gameId, string token, Direction direction)
        {
            _server.Play(gameId, token, direction);
            Assert.That(_apiResponse.HasError, Is.False, _apiResponse.ErrorMessage);
            Assert.That(_apiResponse.ErrorMessage, Is.Null);
            Assert.That(_apiResponse.Text, Is.Not.Null);
            return _apiResponse.Text.JsonToObject<GameResponse>();
        }

        private void AssertPlayError(string gameId, string token, Direction direction, string message)
        {
            _server.Play(gameId, token, direction);
            Assert.That(_apiResponse.HasError, Is.True);
            Assert.That(_apiResponse.ErrorMessage, Is.EqualTo(message));
            Assert.That(_apiResponse.Text, Is.Null);
        }

        private void AssertPlayHasMapText(string gameId, string token, Direction direction, string mapText)
        {
            _server.Play(gameId, token, direction);
            var response = _apiResponse.Text.JsonToObject<GameResponse>();
            Assert.That(response.Game.Board.MapText, Is.EqualTo(mapText));
        }


        private GameResponse Start(string mapText)

        {
            _mockMapMaker.MapText = mapText;
            _server.StartArena();
            return _apiResponse.Text.JsonToObject<GameResponse>();
        }

        [Test]
        public void KillEnemy()
        {
            _mockMapMaker.MapText = "@2$2@1$2";
            _server.StartArena();

            string text = _apiResponse.Text;
            var response = text.JsonToObject<GameResponse>();
            for (int i = 0; i < 5; i++)
                response = Play(response.Game.Id, response.Token, Direction.North);

            Hero player2 = response.Game.Players.First(p => p.Id == 2);
            Assert.That(player2.Life, Is.EqualTo(100));
            Assert.That(player2.MineCount, Is.EqualTo(0));
            Assert.That(response.Self.MineCount, Is.EqualTo(2));
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2$1@1$1"));
        }

        [Test]
        public void PlayWithWrongGame()
        {
            GameResponse response = Start("  ##@1##");
            AssertPlayError(response.Game.Id + "bad", response.Token, Direction.North, "Unable to find the game");
        }

        [Test]
        public void PlayWithWrongToken()
        {
            GameResponse response = Start("  ##@1##");
            AssertPlayError(response.Game.Id, response.Token + "bad", Direction.North,
                "Unable to find the token in your game");
        }

        [Test]
        public void SpawnOnDeath()
        {
            GameResponse response = Start("$-    @1");
            for (int i = 0; i < 80; i++)
                Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Self.Life, Is.EqualTo(19));
            response = Play(response.Game.Id, response.Token, Direction.North);

            Assert.That(response.Self.Life, Is.EqualTo(100));
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$-    @1"));
        }

        [Test]
        public void SpawnOnPlayer()
        {
            GameResponse response = Start("@1@2@3@4");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            _mockGameStateProvider.Game.Game.Board.MapText = "@2@3@4@1";
            _mockGameStateProvider.Game.Game.Players.First(p => p.Id == 1).Pos = new Pos {X = 2, Y = 2};
            _mockGameStateProvider.Game.Game.Players.First(p => p.Id == 2).Pos = new Pos {X = 1, Y = 1};
            _mockGameStateProvider.Game.Game.Players.First(p => p.Id == 3).Pos = new Pos {X = 2, Y = 1};
            _mockGameStateProvider.Game.Game.Players.First(p => p.Id == 4).Pos = new Pos {X = 1, Y = 2};

            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2@3@4@1"));
            response = Play(response.Game.Id, response.Token, Direction.West);
            response = Play(response.Game.Id, response.Token, Direction.West);
            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Game.Players.First(p => p.Id == 4).Life, Is.EqualTo(20));
            response = Play(response.Game.Id, response.Token, Direction.West);


            Assert.That(response.Game.Board.MapText, Is.EqualTo("@1@2@3@4"));
        }

        [Test]
        public void Steps()
        {
            GameResponse response = Start("  ##@1##");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@1##  ##"));
        }

        [Test]
        public void StepsIntoGoldMine()
        {
            GameResponse response = Start("$-  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
        }

        [Test]
        public void StepsIntoGoldMineAndDies()
        {
            GameResponse response = Start("$-  @1  ");
            for (int i = 0; i < 100; i++) Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$-  @1  "));
            Assert.That(response.Game.Players[0].Life, Is.EqualTo(100));
            Assert.That(response.Self.MineCount, Is.EqualTo(0));
        }

        [Test]
        public void StepsIntoGoldMineBelongsToSelf()
        {
            GameResponse response = Start("$1  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
            Assert.That(response.Self.Life, Is.EqualTo(99));
        }

        [Test]
        public void StepsIntoGoldMineOfEnemy()
        {
            GameResponse response = Start("$4@4@1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1@4@1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
            Assert.That(response.Self.Life, Is.EqualTo(79));
            Assert.That(response.Game.Players.First(p => p.Id == 4).MineCount, Is.EqualTo(0));
        }

        [Test]
        public void StepsIntoPlayerAndDamages()
        {
            GameResponse response = Start("@2  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2  @1  "));
            Assert.That(response.Game.Players.Where(p => p.Id == 2), Has.All.Property("Life").EqualTo(80));
            Assert.That(response.Self.Life, Is.EqualTo(99));
        }

        [Test]
        public void StepsIntoTavern()
        {
            GameResponse response = Start("[]$1@1  ");
            for (int i = 0; i < 51; i++)
            {
                response = Play(response.Game.Id, response.Token, Direction.Stay);
            }
            int life = response.Self.Life;

            Assert.That(response.Self.Life, Is.LessThan(50));
            int gold = response.Self.Gold;
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("[]$1@1  "));
            Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
            Assert.That(response.Self.Life, Is.EqualTo((life + 50) - 1));
        }

        [Test]
        public void StepsIntoTavernOverdrinking()
        {
            GameResponse response = Start("[]$1@1  ");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.Stay);

            int gold = response.Self.Gold;

            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(99));
            Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
        }

        [Test]
        public void StepsIntoTavernWithoutPayment()
        {
            GameResponse response = Start("[]  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.North);

            Assert.That(response.Self.Life, Is.EqualTo(98));
            Assert.That(response.Self.Gold, Is.EqualTo(0));
        }

        [Test]
        public void StepsOutOfMap()
        {
            GameResponse response = Start("@1      ");
            string token = response.Token;
            string gameId = response.Game.Id;

            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
            AssertPlayHasMapText(gameId, token, Direction.East, "  @1    ");
            AssertPlayHasMapText(gameId, token, Direction.East, "  @1    ");
            AssertPlayHasMapText(gameId, token, Direction.South, "      @1");
            AssertPlayHasMapText(gameId, token, Direction.South, "      @1");
            AssertPlayHasMapText(gameId, token, Direction.West, "    @1  ");
            AssertPlayHasMapText(gameId, token, Direction.West, "    @1  ");
            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
        }

        [Test]
        public void StepsOverTree()
        {
            GameResponse response = Start("##  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("##  @1  "));
        }

        [Test]
        public void ThirstDoesNotKill()
        {
            GameResponse response = Start("  ##@1##");

            for (int i = 0; i < 100; i++)
                response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(1));
        }

        [Test]
        public void ThirstIsApplied()
        {
            GameResponse response = Start("  ##@1##");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(99));
            Assert.That(response.Game.Players.First(p => p.Id == response.Self.Id).Life, Is.EqualTo(99));
        }

        [Test]
        public void TurnIncrementsByFour()
        {
            GameResponse response = Start("  ##@1##");
            int turn = response.Game.Turn;
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Turn, Is.EqualTo(turn + 4));
        }
    }
}
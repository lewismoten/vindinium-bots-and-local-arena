﻿using System;
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
            IGameServerProxy server = new GameServer(mockMapMaker, new MockApiResponse());
            _gameResponse = server.StartArena().JsonToObject<GameResponse>();
            _game = _gameResponse.Game;

            Console.WriteLine(_game.Board);
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
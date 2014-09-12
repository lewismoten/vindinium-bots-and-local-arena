﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public class GameServer
    {
        private const string Tavern = "[]";
        private const string MinePrefix = "$";
        private const string UnclaimedMine = "$-";
        private const string OpenPath = "  ";
        private const string PlayerPrefix = "@";
        private GameResponse _response = new GameResponse();

        public string Start()
        {
            return Start(
                "        [][]        ##                ##$-  @1$-####$-@4  $-##  ##        ##  ##                                        ##  ##        ##  ##$-  @2$-####$-@3  $-##                ##        [][]        ");
        }

        public string Start(string mapText)
        {
            var grid = new Grid {MapText = mapText};
            _response = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = new Board {MapText = grid.MapText, Size = grid.Size},
                    Finished = false,
                    Id = "the-game-id",
                    MaxTurns = 20,
                    Players = new List<Hero>
                    {
                        CreateHero(mapText, grid, 1),
                        CreateHero(mapText, grid, 2),
                        CreateHero(mapText, grid, 3),
                        CreateHero(mapText, grid, 4)
                    },
                    Turn = 0
                },
                PlayUrl = "http://vindinium.org/api/the-game-id/the-token/play",
                Self = CreateHero(mapText, grid, 1),
                Token = "the-token",
                ViewUrl = "http://vindinium.org/the-game-id"
            };
            return _response.ToJson();
        }

        private static Hero CreateHero(string mapText, Grid grid, int playerId)
        {
            string heroToken = string.Format("{0}{1}", PlayerPrefix, playerId);
            string mineToken = string.Format("{0}{1}", MinePrefix, playerId);
            Pos position = grid.PositionOf(heroToken);
            int mineCount = Regex.Matches(mapText, Regex.Escape(mineToken)).Count;

            return new Hero
            {
                Id = playerId,
                Name = playerId == 1 ? "GrimTrick" : "random",
                UserId = playerId == 1 ? "8aq2nq2b" : null,
                Elo = 1213,
                Pos = position,
                Life = 100,
                Gold = 0,
                MineCount = mineCount,
                SpawnPos = position,
                Crashed = false
            };
        }

        public string Play(string token, Direction direction)
        {
            var map = new Grid {MapText = _response.Game.Board.MapText};
            lock (map.SynchronizationRoot)
            {
                if (_response.Self.Life > 1)
                {
                    _response.Self.Life--;
                }


                Pos playerPos = _response.Self.Pos;
                string playerToken = map[playerPos];
                Pos targetPos = playerPos + GetTargetOffset(direction);
                KeepPositionOnMap(targetPos, _response.Game.Board.Size);
                string targetToken = map[targetPos];

                PlayerMoving(playerPos, map, targetToken, targetPos);
                PlayerMoved(direction, targetToken, playerToken);

                _response.Game.Players.AsParallel().ForAll(p => p.Gold += p.MineCount);
                _response.Game.Board.MapText = map.MapText;
                _response.Game.Players[0].Life = _response.Self.Life;
                _response.Game.Players[0].MineCount = _response.Self.MineCount;
                _response.Self.Gold = _response.Game.Players.First(p => p.Id == _response.Self.Id).Gold;
                _response.Self.Pos = _response.Game.Players.First(p => p.Id == _response.Self.Id).Pos;
            }


            return _response.ToJson();
        }

        private void PlayerMoved(Direction direction, string targetToken, string playerToken)
        {
            if (targetToken.StartsWith(PlayerPrefix) && direction != Direction.Stay && targetToken != playerToken)
            {
                int enemyId = int.Parse(targetToken.Substring(1));
                _response.Game.Players.Where(p => p.Id == enemyId).AsParallel().ForAll(p => p.Life -= 20);
            }
        }

        private void PlayerMoving(Pos playerPos, Grid map, string targetToken, Pos targetPos)
        {
            if (targetToken == OpenPath)
            {
                StepOntoPath(targetToken, targetPos, playerPos, map);
            }
            else if (targetToken == Tavern)
            {
                StepIntoTavern();
            }
            else if (targetToken.StartsWith(MinePrefix))
            {
                StepIntoMine(map, targetPos, targetToken);
            }
        }

        private static void KeepPositionOnMap(Pos position, int mapSize)
        {
            if (position.X < 1) position.X = 1;
            if (position.Y < 1) position.Y = 1;
            if (position.X > mapSize) position.X = mapSize;
            if (position.Y > mapSize) position.Y = mapSize;
        }

        private void StepOntoPath(string targetToken, Pos targetPos, Pos playerPos, Grid map)
        {
            string playerToken = map[playerPos];
            map[targetPos] = playerToken;
            map[playerPos] = targetToken;
            _response.Game.Players.Where(p => string.Format("{0}{1}", PlayerPrefix, p.Id) == playerToken)
                .AsParallel()
                .ForAll(
                    p => p.Pos = targetPos);
        }

        private void StepIntoMine(Grid map, Pos targetPos, string targetToken)
        {
            string playerMine = string.Format("{0}{1}", MinePrefix, _response.Self.Id);
            if (targetToken != playerMine)
            {
                if (_response.Self.Life <= 20)
                {
                    _response.Self.Life = 100;
                }
                else
                {
                    _response.Self.Life -= 20;
                    _response.Self.MineCount++;
                    if (targetToken != UnclaimedMine)
                    {
                        int playerId = int.Parse(targetToken.Substring(1, 1));
                        _response.Game.Players.Where(p => p.Id == playerId).AsParallel().ForAll(p => p.MineCount--);
                    }
                    map[targetPos] = playerMine;
                }
            }
        }

        private void StepIntoTavern()
        {
            if (_response.Self.Gold >= 2)
            {
                _response.Self.Life += 50;
                _response.Game.Players.First(p => p.Id == _response.Self.Id).Gold
                    -= 2;
                if (_response.Self.Life > 100)
                {
                    _response.Self.Life = 100;
                }
            }
        }

        private Pos GetTargetOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Pos {Y = -1};
                case Direction.South:
                    return new Pos {Y = 1};
                case Direction.West:
                    return new Pos {X = -1};
                case Direction.East:
                    return new Pos {X = 1};
                default:
                    return new Pos();
            }
        }


        public string Start(EnvironmentType environmentType)
        {
            Start();
            if (environmentType == EnvironmentType.Training)
            {
                _response.Game.Players.Where(p => p.Id != _response.Self.Id).ToList().ForEach(p => p.Elo = null);
            }
            return _response.ToJson();
        }
    }
}
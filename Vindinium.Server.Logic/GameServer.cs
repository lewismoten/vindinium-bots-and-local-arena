using System;
using System.Collections.Generic;
using System.Linq;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Game.Logic
{
    public class GameServer : IGameServerProxy
    {
        private const int FullLife = 100;
        private const int HealingCost = 2;
        private const int AttackDamage = 20;
        private readonly IBoardHelper _boardHelper;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IMapMaker _mapMaker;

        public GameServer(IMapMaker mapMaker, IApiResponse apiResponse, IGameStateProvider gameStateProvider,
            IBoardHelper boardHelper)
        {
            _mapMaker = mapMaker;
            Response = apiResponse;
            _gameStateProvider = gameStateProvider;
            _boardHelper = boardHelper;
        }

        public IApiResponse Response { get; private set; }

        public void Play(string gameId, string token, Direction direction)
        {
            GameResponse gameResponse = _gameStateProvider.Game;

            if (token != gameResponse.Token)
            {
                SetError("Unable to find the token in your game");
                return;
            }

            Common.DataStructures.Game game = gameResponse.Game;

            if (gameId != game.Id)
            {
                SetError("Unable to find the game");
                return;
            }

            if (game.Finished)
            {
                SetError("Game has finished");
                return;
            }


            Hero self = gameResponse.Self;

            if (self.Crashed)
            {
                SetError("You have crashed and can no longer play");
                return;
            }

            _boardHelper.MapText = game.Board.MapText;

            List<Hero> players = game.Players;
            Hero player = players.First(p => p.Id == self.Id);
            Pos playerPos = player.Pos;
            Pos targetPos = playerPos + GetTargetOffset(direction);
            KeepPositionOnMap(targetPos, _boardHelper.Size);
            string targetToken = _boardHelper[targetPos];

            if (direction != Direction.Stay)
            {
                PlayerMoving(playerPos, _boardHelper, targetToken, targetPos, player);
                PlayerMoved(direction, targetToken, player, _boardHelper);
            }

            _boardHelper.RespawnDeadPlayers(players);
            player.GetThirsty();
            players.RaiseTheDead();
            players.ForEach(p => p.AssignPosAndMinesFromMap(_boardHelper));
            game.Board.MapText = _boardHelper.MapText;
            player.GetWealthy();
            gameResponse.Self = player;

            game.Turn += 4;
            if (game.Turn >= game.MaxTurns)
            {
                game.Finished = true;
            }
            Response.ErrorMessage = null;
            Response.HasError = false;
            Response.Text = gameResponse.ToJson();
        }

        public void StartTraining(uint rounds)
        {
            Start(EnvironmentType.Training, rounds);
        }

        public void StartArena()
        {
            Start(EnvironmentType.Arena, 300);
        }

        private void SetError(string message)
        {
            Response.Text = null;
            Response.ErrorMessage = message;
            Response.HasError = true;
        }

        private void CreateGame()
        {
            string gameId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string token = Guid.NewGuid().ToString("N").Substring(0, 8);

            _gameStateProvider.Game = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = new Board
                    {
                        MapText = _boardHelper.MapText,
                        Size = _boardHelper.Size
                    },
                    Finished = false,
                    Id = gameId,
                    MaxTurns = 20,
                    Players = new List<Hero>(),
                    Turn = 0
                },
                PlayUrl = string.Format("http://vindinium.org/api/{0}/{1}/play", gameId, token),
                Self = CreateHero(1),
                Token = token,
                ViewUrl = string.Format("http://vindinium.org/{0}", gameId)
            };
            for (int i = 1; i <= 4; i++)
            {
                _gameStateProvider.Game.Game.Players.Add(CreateHero(i));
            }
        }

        private void Start()
        {
            _mapMaker.GenerateMap(_boardHelper);
            CreateGame();
        }

        private Hero CreateHero(int playerId)
        {
            var hero = new Hero
            {
                Id = playerId,
                Name = playerId == 1 ? "GrimTrick" : "random",
                UserId = playerId == 1 ? "8aq2nq2b" : null,
                Elo = 1213,
                Life = FullLife,
                Gold = 0,
                Crashed = false
            };
            hero.AssignPosAndMinesFromMap(_boardHelper);
            hero.SpawnPos = hero.Pos;
            return hero;
        }

        private void PlayerMoved(Direction direction, string targetToken, Hero player, IBoardHelper map)
        {
            if (SteppedIntoEnemy(player, direction, targetToken))
            {
                int enemyId = int.Parse(targetToken.Substring(1));
                Hero enemy = _gameStateProvider.Game.Game.Players.First(p => p.Id == enemyId);
                player.Attack(enemy, map);
            }
        }

        private static bool SteppedIntoEnemy(Hero player, Direction direction, string targetToken)
        {
            return TokenHelper.IsPlayer(targetToken) && direction != Direction.Stay &&
                   targetToken != player.PlayerToken();
        }

        private void PlayerMoving(Pos playerPos, IBoardHelper map, string targetToken, Pos targetPos, Hero player)
        {
            if (targetToken == TokenHelper.OpenPath)
            {
                StepOntoPath(targetToken, targetPos, playerPos, map);
            }
            else if (targetToken == TokenHelper.Tavern)
            {
                _gameStateProvider.Game.Self.Purchase(HealingCost, hero => hero.Heal());
            }
            else if (TokenHelper.IsMine(targetToken))
            {
                StepIntoMine(map, targetPos, targetToken, player);
            }
        }

        private static void KeepPositionOnMap(Pos position, int mapSize)
        {
            if (position.X < 1) position.X = 1;
            if (position.Y < 1) position.Y = 1;
            if (position.X > mapSize) position.X = mapSize;
            if (position.Y > mapSize) position.Y = mapSize;
        }

        private void StepOntoPath(string targetToken, Pos targetPos, Pos playerPos, IBoardHelper map)
        {
            string playerToken = map[playerPos];
            map[targetPos] = playerToken;
            map[playerPos] = targetToken;
            _gameStateProvider.Game.Game.Players.Where(p => p.PlayerToken() == playerToken)
                .AsParallel()
                .ForAll(
                    p => p.Pos = targetPos);
        }

        private void StepIntoMine(IBoardHelper map, Pos targetPos, string targetToken, Hero player)
        {
            if (targetToken != player.MineToken())
            {
                player.Life -= AttackDamage;
                if (player.IsLiving())
                {
                    map[targetPos] = player.MineToken();
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


        private void Start(EnvironmentType environmentType, uint rounds)
        {
            Start();
            if (environmentType == EnvironmentType.Training)
            {
                _gameStateProvider.Game.Game.Players.Where(p => p.Id != _gameStateProvider.Game.Self.Id)
                    .ToList()
                    .ForEach(p => p.Elo = null);
            }

            _gameStateProvider.Game.Game.MaxTurns = rounds*4;

            Response.HasError = false;
            Response.ErrorMessage = null;
            Response.Text = _gameStateProvider.Game.ToJson();
        }
    }
}
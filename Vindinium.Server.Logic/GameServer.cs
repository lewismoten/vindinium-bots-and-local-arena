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
        private readonly IApiResponse _apiResponse;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IMapMaker _mapMaker;

        public GameServer(IMapMaker mapMaker, IApiResponse apiResponse, IGameStateProvider gameStateProvider)
        {
            _mapMaker = mapMaker;
            _apiResponse = apiResponse;
            _gameStateProvider = gameStateProvider;
        }

        public GameResponse GameResponse
        {
            get { return null; }
        }

        public string Play(string gameId, string token, Direction direction)
        {
            _apiResponse.ErrorMessage = null;
            _apiResponse.HasError = false;
            _apiResponse.Text = null;

            if (token != _gameStateProvider.Game.Token)
            {
                _apiResponse.ErrorMessage = "Unable to find the token in your game";
                _apiResponse.HasError = true;
                return _apiResponse.ErrorMessage;
            }
            if (gameId != _gameStateProvider.Game.Game.Id)
            {
                _apiResponse.ErrorMessage = "Unable to find the game";
                _apiResponse.HasError = true;
                return _apiResponse.ErrorMessage;
            }
            Board board = _gameStateProvider.Game.Game.Board;
            IBoardHelper boardHelper = new BoardHelper(_gameStateProvider.Game.Game.Board);

            List<Hero> players = _gameStateProvider.Game.Game.Players;
            Hero player = players.First(p => p.Id == _gameStateProvider.Game.Self.Id);
            Pos playerPos = player.Pos;
            Pos targetPos = playerPos + GetTargetOffset(direction);
            KeepPositionOnMap(targetPos, board.Size);
            string targetToken = boardHelper[targetPos];

            PlayerMoving(playerPos, boardHelper, targetToken, targetPos, player);
            PlayerMoved(direction, targetToken, player, boardHelper);
            MoveDeadPlayers(boardHelper, players);
            player.GetThirsty();
            players.RaiseTheDead();
            players.ForEach(p => p.AssignPosAndMinesFromMap(boardHelper));
            board.MapText = boardHelper.MapText;
            player.GetWealthy();
            _gameStateProvider.Game.Self = player;
            _apiResponse.Text = _gameStateProvider.Game.ToJson();
            return _apiResponse.Text;
        }

        public string StartTraining(uint turns)
        {
            return Start(EnvironmentType.Training);
        }

        public string StartArena()
        {
            return Start(EnvironmentType.Arena);
        }

        private void Start(IBoardHelper boardHelper)
        {
            string gameId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string token = Guid.NewGuid().ToString("N").Substring(0, 8);

            _gameStateProvider.Game = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = boardHelper.UnderlyingBoard,
                    Finished = false,
                    Id = gameId,
                    MaxTurns = 20,
                    Players = new List<Hero>(),
                    Turn = 0
                },
                PlayUrl = string.Format("http://vindinium.org/api/{0}/{1}/play", gameId, token),
                Self = CreateHero(boardHelper, 1),
                Token = token,
                ViewUrl = string.Format("http://vindinium.org/{0}", gameId)
            };
            for (int i = 1; i <= 4; i++)
            {
                _gameStateProvider.Game.Game.Players.Add(CreateHero(boardHelper, i));
            }
        }

        private void MoveDeadPlayers(IBoardHelper map, List<Hero> players)
        {
            Hero[] misplacedDead =
                players.Where(p => p.IsDead() && p.Pos != p.SpawnPos && p.Crashed == false)
                    .ToArray();
            do
            {
                foreach (Hero deadPlayer in misplacedDead)
                {
                    ReplaceMapToken(map, deadPlayer.MineToken(), TokenHelper.NeutralMine);
                    players.Where(p => p.Pos == deadPlayer.SpawnPos)
                        .ToList()
                        .ForEach(p => p.Die());
                    map[deadPlayer.Pos] = TokenHelper.OpenPath;
                    deadPlayer.Pos = deadPlayer.SpawnPos;
                }
                misplacedDead =
                    players.Where(p => p.IsDead() && p.Pos != p.SpawnPos).ToArray();
            } while (misplacedDead.Any());

            players.Where(p => p.Crashed == false)
                .ToList()
                .ForEach(p => map[p.Pos] = p.PlayerToken());
        }

        private static void ReplaceMapToken(IBoardHelper map, string oldToken, string newToken)
        {
            map.ForEach(p =>
            {
                if (map[p] == oldToken)
                {
                    map[p] = newToken;
                }
            });
        }

        private void Start()
        {
            var board = new Board();
            var boardHelper = new BoardHelper(board);
            _mapMaker.GenerateMap(boardHelper);
            Start(boardHelper);
        }

        private Hero CreateHero(IBoardHelper boardHelper, int playerId)
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
            hero.AssignPosAndMinesFromMap(boardHelper);
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


        private string Start(EnvironmentType environmentType)
        {
            Start();
            if (environmentType == EnvironmentType.Training)
            {
                _gameStateProvider.Game.Game.Players.Where(p => p.Id != _gameStateProvider.Game.Self.Id)
                    .ToList()
                    .ForEach(p => p.Elo = null);
            }
            return _gameStateProvider.Game.ToJson();
        }
    }
}
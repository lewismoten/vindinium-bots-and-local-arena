using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public class BoardHelper : IBoardHelper
    {
        public Board GetBoard()
        {
            return new Board {MapText = MapText, Size = Size};
        }

        public int Size
        {
            get { return (byte) Math.Sqrt(MapText.Length/2d); }
        }

        public string MapText { get; set; }

        public string this[int x, int y]
        {
            get
            {
                if (!IsOnMap(x, y))
                {
                    return null;
                }
                return MapText.Substring(StringIndex(x, y), 2);
            }
            set
            {
                int index = StringIndex(x, y);
                MapText = MapText.Remove(index, 2).Insert(index, value);
            }
        }

        public string this[Pos pos]
        {
            get { return this[pos.X, pos.Y]; }
            set { this[pos.X, pos.Y] = value; }
        }

        public Pos PositionOf(string token)
        {
            int index = MapText.IndexOf(token, StringComparison.Ordinal);
            if (index == -1) return null;
            index = index/2;

            int x = (index%(Size));
            return new Pos {X = x + 1, Y = ((index - x)/Size + 1)};
        }

        public void ForEach(Action<Pos> action)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    action(new Pos {X = x + 1, Y = y + 1});
                }
            }
        }

        public void MakeSymmetric()
        {
            for (int y = 1; y <= Size/2; y++)
            {
                for (int x = 1; x <= Size/2; x++)
                {
                    string token = this[x, y];
                    int max = Size + 1;

                    if (token.StartsWith("@"))
                    {
                        this[x, y] = TokenHelper.Player(1);
                        this[x, max - y] = TokenHelper.Player(2);
                        this[max - x, y] = TokenHelper.Player(3);
                        this[max - x, max - y] = TokenHelper.Player(4);
                    }
                    else
                    {
                        this[max - x, y] = token;
                        this[max - x, max - y] = token;
                        this[x, max - y] = token;
                    }
                }
            }
        }

        public int TokenCount(string token)
        {
            return Regex.Matches(MapText, Regex.Escape(token)).Count;
        }

        public AdjacentTokens GetAdjacentTokens(Pos pos)
        {
            return new AdjacentTokens
            {
                Stay = GetTokenPosition(pos),
                North = GetTokenPosition(pos + new Pos {Y = -1}),
                South = GetTokenPosition(pos + new Pos {Y = 1}),
                East = GetTokenPosition(pos + new Pos {X = 1}),
                West = GetTokenPosition(pos + new Pos {X = -1})
            };
        }

        public void ReplaceTokens(string oldToken, string newToken)
        {
            MapText = MapText.Replace(oldToken, newToken);
        }


        public void RespawnDeadPlayers(List<Hero> players)
        {
            Hero[] misplacedDead =
                players.Where(p => p.IsDead() && p.Pos != p.SpawnPos && p.Crashed == false)
                    .ToArray();
            do
            {
                foreach (Hero deadPlayer in misplacedDead)
                {
                    ReplaceTokens(deadPlayer.MineToken(), TokenHelper.NeutralMine);

                    players.Where(p => p.Pos == deadPlayer.SpawnPos)
                        .ToList()
                        .ForEach(p => p.Die());
                    this[deadPlayer.Pos] = TokenHelper.OpenPath;
                    deadPlayer.Pos = deadPlayer.SpawnPos;
                }
                misplacedDead =
                    players.Where(p => p.IsDead() && p.Pos != p.SpawnPos).ToArray();
            } while (misplacedDead.Any());

            players.Where(p => p.Crashed == false)
                .ToList()
                .ForEach(p => this[p.Pos] = p.PlayerToken());
        }

        private int StringIndex(int x, int y)
        {
            y = y - 1;
            x = x - 1;
            return (y*Size*2) + (x*2);
        }

        public bool PathExistsBetween(Pos start, Pos end)
        {
            if (start == end) return true;
            if (start + new Pos {X = 1} == end) return true;
            if (start + new Pos {X = -1} == end) return true;
            if (start + new Pos {Y = 1} == end) return true;
            if (start + new Pos {Y = -1} == end) return true;


            return false;
        }

        private TokenPosition GetTokenPosition(Pos position)
        {
            return IsOnMap(position)
                ? new TokenPosition {Position = position, Token = this[position]}
                : new TokenPosition();
        }

        private bool IsOnMap(int x, int y)
        {
            return y >= 1
                   && y <= Size
                   && x >= 1
                   && x <= Size;
        }

        private bool IsOnMap(Pos position)
        {
            return IsOnMap(position.X, position.Y);
        }

        public override string ToString()
        {
            return MapFormatter.FormatTokensAsMap(MapText);
        }
    }
}
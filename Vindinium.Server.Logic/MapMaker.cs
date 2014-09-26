using System;
using System.Collections.Generic;
using System.Linq;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public class MapMaker : IMapMaker
    {
        private const string EdgeToken = "??";

        public string GenerateMap(int seed)
        {
            IBoardHelper boardHelper = new BoardHelper(new Board());
            var random = new Random(seed);
            int quarter = random.Next(5, 14);
            int size = quarter*2;
            boardHelper.MapText = string.Empty.PadLeft(size*size*2, '#');

            MakePathBetweenQuadrants(boardHelper, random);

            AddMines(boardHelper, random);
            AddTavern(boardHelper, random);
            AddPlayer(boardHelper, random);

            boardHelper.ForEach(p => { if (boardHelper[p] == EdgeToken) boardHelper[p] = TokenHelper.Obstruction; });

            boardHelper.MakeSymmetric();
            return boardHelper.MapText;
        }

        private static void AddMines(IBoardHelper boardHelper, Random random)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);

            int count = random.Next(1, edges.Count());

            for (int i = 0; i < count; i++)
            {
                edges = FindTokenPositions(boardHelper, EdgeToken);
                int edgeIndex = random.Next(0, edges.Count());
                Pos pos = edges[edgeIndex];
                boardHelper[pos] = TokenHelper.NeutralMine;
            }
        }

        private static void AddTavern(IBoardHelper boardHelper, Random random)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);
            int edgeIndex = random.Next(0, edges.Count());
            Pos pos = edges[edgeIndex];
            boardHelper[pos] = TokenHelper.Tavern;
        }

        private static void AddPlayer(IBoardHelper boardHelper, Random random)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, TokenHelper.OpenPath);
            int edgeIndex = random.Next(0, edges.Count());
            Pos pos = edges[edgeIndex];
            boardHelper[pos] = TokenHelper.Player(0);
        }

        private static List<Pos> FindTokenPositions(IBoardHelper boardHelper, string token)
        {
            int quarter = boardHelper.Size/2;
            var positions = new List<Pos>();
            boardHelper.ForEach(
                p => { if (p.X <= quarter && p.Y <= quarter && boardHelper[p] == token) positions.Add(p); });
            return positions;
        }

        private static void MakePathBetweenQuadrants(IBoardHelper boardHelper, Random random)
        {
            int quarter = boardHelper.Size/2;
            var pos = new Pos {Y = quarter, X = random.Next(1, quarter + 1)};
            AdjacentTokens tokenPositions = boardHelper.GetAdjacentTokens(pos);
            OpenPathAndMarkEdges(boardHelper, tokenPositions);
            do
            {
                List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);
                int edgeIndex = random.Next(0, edges.Count());
                pos = edges[edgeIndex];
                tokenPositions = boardHelper.GetAdjacentTokens(pos);
                OpenPathAndMarkEdges(boardHelper, tokenPositions);
            } while (pos.X != quarter);
        }

        private static void OpenPathAndMarkEdges(IBoardHelper boardHelper, AdjacentTokens tokenPositions)
        {
            boardHelper[tokenPositions.Stay.Position] = TokenHelper.OpenPath;

            if (tokenPositions.North.Token == TokenHelper.Obstruction)
            {
                boardHelper[tokenPositions.North.Position] = EdgeToken;
            }
            if (tokenPositions.East.Token == TokenHelper.Obstruction)
            {
                boardHelper[tokenPositions.East.Position] = EdgeToken;
            }
            if (tokenPositions.West.Token == TokenHelper.Obstruction)
            {
                boardHelper[tokenPositions.West.Position] = EdgeToken;
            }
            if (tokenPositions.South.Token == TokenHelper.Obstruction)
            {
                boardHelper[tokenPositions.South.Position] = EdgeToken;
            }
        }
    }
}
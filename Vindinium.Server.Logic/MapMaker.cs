using System.Collections.Generic;
using System.Linq;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public class MapMaker : IMapMaker
    {
        private const string EdgeToken = "??";
        private readonly IRandomizer _randomizer;

        public MapMaker(IRandomizer randomizer)
        {
            _randomizer = randomizer;
        }

        public void GenerateMap(IBoardHelper boardHelper)
        {
            int quarter = _randomizer.Next(5, 14);
            int size = quarter*2;
            boardHelper.MapText = string.Empty.PadLeft(size*size*2, '#');

            MakePathBetweenQuadrants(boardHelper);

            AddMines(boardHelper);
            AddTavern(boardHelper);
            AddPlayer(boardHelper);

            boardHelper.ForEach(p => { if (boardHelper[p] == EdgeToken) boardHelper[p] = TokenHelper.Obstruction; });

            boardHelper.MakeSymmetric();
        }

        private void AddMines(IBoardHelper boardHelper)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);

            int count = _randomizer.Next(1, edges.Count());

            for (int i = 0; i < count; i++)
            {
                edges = FindTokenPositions(boardHelper, EdgeToken);
                int edgeIndex = _randomizer.Next(0, edges.Count());
                Pos pos = edges[edgeIndex];
                boardHelper[pos] = TokenHelper.NeutralMine;
            }
        }

        private void AddTavern(IBoardHelper boardHelper)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);
            int edgeIndex = _randomizer.Next(0, edges.Count());
            Pos pos = edges[edgeIndex];
            boardHelper[pos] = TokenHelper.Tavern;
        }

        private void AddPlayer(IBoardHelper boardHelper)
        {
            List<Pos> edges = FindTokenPositions(boardHelper, TokenHelper.OpenPath);
            int edgeIndex = _randomizer.Next(0, edges.Count());
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

        private void MakePathBetweenQuadrants(IBoardHelper boardHelper)
        {
            int quarter = boardHelper.Size/2;
            var pos = new Pos {Y = quarter, X = _randomizer.Next(1, quarter + 1)};
            AdjacentTokens tokenPositions = boardHelper.GetAdjacentTokens(pos);
            OpenPathAndMarkEdges(boardHelper, tokenPositions);
            do
            {
                List<Pos> edges = FindTokenPositions(boardHelper, EdgeToken);
                int edgeIndex = _randomizer.Next(0, edges.Count());
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
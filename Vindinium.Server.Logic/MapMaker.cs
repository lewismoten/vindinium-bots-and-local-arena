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
            var grid = new Grid();
            var random = new Random(seed);
            int quarter = random.Next(5, 14);
            int size = quarter*2;
            grid.MapText = string.Empty.PadLeft(size*size*2, '#');

            MakePathBetweenQuadrants(grid, random);
            AddMines(grid, random);
            AddTavern(grid, random);
            AddPlayer(grid, random);

            grid.ForEach(p => { if (grid[p] == EdgeToken) grid[p] = TokenHelper.Obstruction; });

            grid.MakeSymmetric();
            return grid.MapText;
        }

        private static void AddMines(Grid grid, Random random)
        {
            List<Pos> edges = FindTokenPositions(grid, EdgeToken);

            int count = random.Next(1, edges.Count());

            for (int i = 0; i < count; i++)
            {
                edges = FindTokenPositions(grid, EdgeToken);
                int edgeIndex = random.Next(0, edges.Count());
                Pos pos = edges[edgeIndex];
                grid[pos] = TokenHelper.NeutralMine;
            }
        }

        private static void AddTavern(Grid grid, Random random)
        {
            List<Pos> edges = FindTokenPositions(grid, EdgeToken);
            int edgeIndex = random.Next(0, edges.Count());
            Pos pos = edges[edgeIndex];
            grid[pos] = TokenHelper.Tavern;
        }

        private static void AddPlayer(Grid grid, Random random)
        {
            List<Pos> edges = FindTokenPositions(grid, TokenHelper.OpenPath);
            int edgeIndex = random.Next(0, edges.Count());
            Pos pos = edges[edgeIndex];
            grid[pos] = TokenHelper.Player(0);
        }

        private static List<Pos> FindTokenPositions(Grid grid, string token)
        {
            int quarter = grid.Size/2;
            var positions = new List<Pos>();
            grid.ForEach(p => { if (p.X <= quarter && p.Y <= quarter && grid[p] == token) positions.Add(p); });
            return positions;
        }

        private static void MakePathBetweenQuadrants(Grid grid, Random random)
        {
            int quarter = grid.Size/2;
            var pos = new Pos {Y = quarter, X = random.Next(1, quarter + 1)};
            AdjacentTokens tokenPositions = grid.GetAdjacentTokens(pos);
            OpenPathAndMarkEdges(grid, tokenPositions);
            do
            {
                List<Pos> edges = FindTokenPositions(grid, EdgeToken);
                int edgeIndex = random.Next(0, edges.Count());
                pos = edges[edgeIndex];
                tokenPositions = grid.GetAdjacentTokens(pos);
                OpenPathAndMarkEdges(grid, tokenPositions);
            } while (pos.X != quarter);
        }

        private static void OpenPathAndMarkEdges(Grid grid, AdjacentTokens tokenPositions)
        {
            grid[tokenPositions.Stay.Position] = TokenHelper.OpenPath;

            if (tokenPositions.North.Token == TokenHelper.Obstruction)
            {
                grid[tokenPositions.North.Position] = EdgeToken;
            }
            if (tokenPositions.East.Token == TokenHelper.Obstruction)
            {
                grid[tokenPositions.East.Position] = EdgeToken;
            }
            if (tokenPositions.West.Token == TokenHelper.Obstruction)
            {
                grid[tokenPositions.West.Position] = EdgeToken;
            }
            if (tokenPositions.South.Token == TokenHelper.Obstruction)
            {
                grid[tokenPositions.South.Position] = EdgeToken;
            }
        }
    }
}
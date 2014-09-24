using System;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public static class MapMaker
    {
        public static string GenerateMap(int seed)
        {
            var grid = new Grid();
            var random = new Random(seed);
            int quarter = random.Next(5, 14);
            int size = quarter*2;
            grid.MapText = "".PadLeft(size*size*2, '#');

            var pos = new Pos {Y = quarter, X = random.Next(1, quarter + 1)};
            grid[pos] = TokenHelper.OpenPath;

            grid.MakeSymmetric();
            return grid.MapText;
        }
    }
}
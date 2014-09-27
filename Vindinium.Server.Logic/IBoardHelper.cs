using System;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public interface IBoardHelper
    {
        string MapText { get; set; }
        int Size { get; }
        string this[Pos position] { get; set; }
        string this[int x, int y] { get; }
        Board UnderlyingBoard { get; }
        void ForEach(Action<Pos> action);
        int TokenCount(string token);
        Pos PositionOf(string token);
        void MakeSymmetric();
        AdjacentTokens GetAdjacentTokens(Pos pos);
    }
}
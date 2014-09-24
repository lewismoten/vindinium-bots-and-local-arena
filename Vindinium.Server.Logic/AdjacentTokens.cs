using Vindinium.Common;

namespace Vindinium.Game.Logic
{
    public class AdjacentTokens
    {
        public TokenPosition North { get; set; }
        public TokenPosition South { get; set; }
        public TokenPosition East { get; set; }
        public TokenPosition West { get; set; }
        public TokenPosition Stay { get; set; }

        public TokenPosition this[Direction direction]
        {
            get
            {
                switch (direction)
                {
                    case Direction.East:
                        return East;
                    case Direction.North:
                        return North;
                    case Direction.South:
                        return South;
                    case Direction.Stay:
                        return Stay;
                    case Direction.West:
                        return West;
                    default:
                        return null;
                }
            }
        }
    }
}
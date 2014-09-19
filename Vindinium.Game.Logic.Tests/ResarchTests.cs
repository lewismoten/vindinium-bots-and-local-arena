using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture, Explicit]
    public class ResarchTests
    {
        [Test, Explicit]
        public void Research()
        {
            for (int size = 10; size < 30; size+=2)
            {

                string path = string.Format(@"C:\vindinium.logs\training\{0:000}\", size);
                var files = Directory.GetFiles(path, "0004.*.json", SearchOption.AllDirectories);
                var boards =
                    files.Select(
                        file =>
                            new Grid {MapText = File.ReadAllText(file).JsonToObject<GameResponse>().Game.Board.MapText})
                        .ToList();
                Console.WriteLine("{0},{1},{2},{3},{4:#0.000}",
                    size,
                    boards.Count(),
                    boards.Min(b=>CountTokens(b)),
                    boards.Max(b=>CountTokens(b)),
                    boards.Average(b=>CountTokens(b)));

                if (size == 14)
                {
                    var text = boards.First(b => CountTokens(b) == 7).MapText;

                    Console.WriteLine(new Board {MapText = text, Size = size});
                }
                /*
Open Path
Size,   Min,    Max,    Avg
10,28,84,58.304
12,24,128,85.829
14,20,176,115.079
16,32,228,157.583
18,32,284,185.209
20,36,356,232.062
22,44,428,275.319
24,48,500,333.903
26,20,588,399.036
28,44,700,444.960

Obstruction
10,4,60,26.560
12,4,108,41.116
14,4,160,62.159
16,8,212,76.222
18,20,276,114.360
20,28,352,140.246
22,36,428,176.652
24,40,512,207.548
26,40,644,236.759
28,40,728,295.413

Mines
10,4,24,7.136
12,4,32,9.054
14,4,28,10.762
16,4,44,14.194
18,4,48,16.432
20,4,64,19.692
22,4,72,24.030
24,4,68,26.548
26,4,80,32.204
28,4,80,35.627
                 */

            }

        }

 
        private static decimal CountTokens(Grid b)
        {
            return (b.TokenCount("$-") + b.TokenCount("$1") + b.TokenCount("$2") + b.TokenCount("$3") +
                   b.TokenCount("$4")) / 4m;
        }
    }
}
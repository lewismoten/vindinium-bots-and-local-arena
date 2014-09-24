using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class MapMakerTests
    {
        private const int SeedCount = 25;
        private const int MaxSeed = int.MaxValue;
        private const int MinSeed = int.MinValue;
        private readonly string[] _openTokens = {"  ", "@1", "@2", "@3", "@4"};

        private static Grid NewMap(int seed)
        {
            return new Grid {MapText = MapMaker.GenerateMap(seed)};
        }

        private static Dictionary<string, int> TokensOnNewMap(int seed)
        {
            Grid map = NewMap(seed);
            var actualTokens = new Dictionary<string, int>();
            map.ForEach(p =>
            {
                string token = map[p];
                if (actualTokens.ContainsKey(token))
                {
                    actualTokens[token]++;
                }
                else
                {
                    actualTokens.Add(token, 1);
                }
            });
            return actualTokens;
        }

        private void AssertTokenIsAlwaysBesideAnother(int seed, string token, string[] acceptableNeighbors)
        {
            var map = new Grid {MapText = MapMaker.GenerateMap(seed)};
            map.ForEach(p =>
            {
                if (map[p] != token) return;
                AdjacentTokens adjacentTokens = map.GetAdjacentTokens(p);
                var actualNeighbors = new[]
                {
                    adjacentTokens.North.Token,
                    adjacentTokens.South.Token,
                    adjacentTokens.East.Token,
                    adjacentTokens.West.Token
                };
                bool found = actualNeighbors.Any(a => acceptableNeighbors.Any(aa => aa == a));

                Assert.That(found, Is.True,
                    "Expected token '{0}' at {1} to be next to tokens '{2}'.\r\n{3}",
                    token,
                    p,
                    string.Join("', '", acceptableNeighbors),
                    map
                    );
            });
        }

        [Test]
        public void MapDoesNotHaveUnexpectedTokens([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            string[] actualTokens = TokensOnNewMap(seed).Select(p => p.Key).ToArray();
            var expectedTokens = new[] {"@1", "@2", "@3", "@4", "$1", "$2", "$3", "$4", "[]", "$-", "##", "  "};
            Assert.That(actualTokens, Is.SubsetOf(expectedTokens));
        }

        [Test]
        public void MapHasEmptyMines([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            string[] actualTokens = TokensOnNewMap(seed).Select(p => p.Key).ToArray();
            Assert.That(actualTokens, Has.Member("$-"));
            Assert.That(actualTokens, Has.No.Member("$1"));
            Assert.That(actualTokens, Has.No.Member("$2"));
            Assert.That(actualTokens, Has.No.Member("$3"));
            Assert.That(actualTokens, Has.No.Member("$4"));
        }

        [Test]
        public void MapHasEmptyPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap(seed);
            Assert.That(actualTokens["  "], Is.AtLeast(1));
        }

        [Test]
        public void MapHasFourTaverns([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap(seed);
            Assert.That(actualTokens["[]"], Is.EqualTo(4));
        }

        [Test]
        public void MapHasImpassibleWoods([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap(seed);
            Assert.That(actualTokens["##"], Is.GreaterThan(0));
        }

        [Test]
        public void MapHasPlayers([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap(seed);
            Assert.That(actualTokens["@1"], Is.EqualTo(1));
            Assert.That(actualTokens["@2"], Is.EqualTo(1));
            Assert.That(actualTokens["@3"], Is.EqualTo(1));
            Assert.That(actualTokens["@4"], Is.EqualTo(1));
        }

        [Test]
        public void MapIsSymmetric([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            string map = MapMaker.GenerateMap(seed);

            map = map.Replace("$-", "$$")
                .Replace("[]", "[[")
                .Replace("@1", "@@")
                .Replace("@2", "@@")
                .Replace("@3", "@@")
                .Replace("@4", "@@");

            Assert.That(map, Is.EqualTo(map.Reverse()));

            int cells = map.Length/2;
            var size = (int) Math.Sqrt(cells);
            int half = size/2;
            int rowLength = size*2;
            for (int i = 0; i < half; i++)
            {
                string row = map.Substring(i*rowLength, rowLength);
                Assert.That(row, Is.EqualTo(row.Reverse()));
            }
        }

        [Test]
        public void MapTextIsExpectedLength([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            string text = MapMaker.GenerateMap(seed);
            int cells = text.Length/2;
            double size = Math.Sqrt(cells);
            Assert.That(size, Is.EqualTo(Math.Floor(size)));
            Assert.That(size, Is.AtLeast(10));
            Assert.That(size, Is.AtMost(28));
            Assert.That(size%2, Is.Not.EqualTo(1));
        }

        [Test]
        public void MinesAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            AssertTokenIsAlwaysBesideAnother(seed, "$-", _openTokens);
        }

        [Test]
        public void OpenPathBetweenLeftAndRightQuadrant([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            Grid map = NewMap(seed);
            int half = map.Size/2;
            for (int y = 1; y <= half; y++)
            {
                if (_openTokens.Any(t => t == map[half, y])) Assert.Pass();
            }
            Assert.Fail("Missing path at top-left quardrent on right edge.\r\n{0}", map);
        }

        [Test]
        public void OpenPathBetweenTopAndBottomQuadrant([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            string tokens = MapMaker.GenerateMap(seed);
            var size = (int) Math.Sqrt(tokens.Length/2.0);

            // first line of tokens in bottom left quadrant
            string borderTokens = tokens.Substring(tokens.Length/2, size);

            Assert.That(_openTokens.Any(t => borderTokens.IndexOf(t, StringComparison.Ordinal) != -1), Is.True,
                "Grid does not have open path on quadrant border\r\n{0}",
                new Grid {MapText = tokens});
        }

        [Test]
        public void OpenPathIsNextToAnotherOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            AssertTokenIsAlwaysBesideAnother(seed, "  ", _openTokens);
        }

        [Test]
        public void PlayersAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            AssertTokenIsAlwaysBesideAnother(seed, "@1", _openTokens);
            AssertTokenIsAlwaysBesideAnother(seed, "@2", _openTokens);
            AssertTokenIsAlwaysBesideAnother(seed, "@3", _openTokens);
            AssertTokenIsAlwaysBesideAnother(seed, "@4", _openTokens);
        }

        [Test]
        public void TavernsAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            AssertTokenIsAlwaysBesideAnother(seed, "[]", _openTokens);
        }
    }
}
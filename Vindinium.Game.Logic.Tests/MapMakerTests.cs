using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vindinium.Common.DataStructures;
using Vindinium.Game.Logic.Tests.Mocks;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class MapMakerTests
    {
        private const int SeedCount = 25;
        private const int MaxSeed = int.MaxValue;
        private const int MinSeed = int.MinValue;
        private readonly string[] _openTokens = {"  ", "@1", "@2", "@3", "@4"};
        private IRandomizer _randomizer;
        private IBoardHelper _boardHelper;
        private IMapMaker _mapMaker;
        private Board _board;

        private Dictionary<string, int> UniqueTokenCounts()
        {
            var actualTokens = new Dictionary<string, int>();
            _boardHelper.ForEach(p =>
            {
                string token = _boardHelper[p];
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

        private void AssertTokenIsAlwaysBesideAnother(string token, string[] acceptableNeighbors)
        {
            _boardHelper.ForEach(p =>
            {
                if (_boardHelper[p] != token) return;
                AdjacentTokens adjacentTokens = _boardHelper.GetAdjacentTokens(p);
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
                    _boardHelper
                    );
            });
        }

        private void GenerateMap(int seed)
        {
            _randomizer = new MockRandomizer(seed);
            _board = new Board();
            _boardHelper = new BoardHelper(_board);
            _mapMaker = new MapMaker(_randomizer);
            _mapMaker.GenerateMap(_boardHelper);
        }

        [Test]
        public void MapDoesNotHaveUnexpectedTokens([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);

            string[] actualTokens = UniqueTokenCounts().Select(p => p.Key).ToArray();
            var expectedTokens = new[] {"@1", "@2", "@3", "@4", "$1", "$2", "$3", "$4", "[]", "$-", "##", "  "};
            Assert.That(actualTokens, Is.SubsetOf(expectedTokens));
        }

        [Test]
        public void MapHasEmptyMines([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            string[] actualTokens = UniqueTokenCounts().Select(p => p.Key).ToArray();
            Assert.That(actualTokens, Has.Member("$-"));
        }

        [Test]
        public void MapHasEmptyPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            Dictionary<string, int> actualTokens = UniqueTokenCounts();
            Assert.That(actualTokens["  "], Is.AtLeast(1));
        }

        [Test]
        public void MapHasFourTaverns([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            Dictionary<string, int> actualTokens = UniqueTokenCounts();
            Assert.That(actualTokens["[]"], Is.EqualTo(4));
        }

        [Test]
        public void MapHasImpassibleWoods([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            Dictionary<string, int> actualTokens = UniqueTokenCounts();
            Assert.That(actualTokens["##"], Is.GreaterThan(0));
        }

        [Test]
        public void MapHasNoPlayerMines([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            string[] actualTokens = UniqueTokenCounts().Select(p => p.Key).ToArray();

            Assert.That(actualTokens,
                Has.No.Member("$1")
                    .And.No.Member("$2")
                    .And.No.Member("$3")
                    .And.No.Member("$4")
                );
        }

        [Test]
        public void MapHasOneOfEachPlayer([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            Dictionary<string, int> statistics = UniqueTokenCounts();
            string[] tokens = statistics.Select(p => string.Format("{0} = {1}", p.Key, p.Value)).ToArray();
            Assert.That(tokens,
                Has.Member("@1 = 1")
                    .And.Member("@2 = 1")
                    .And.Member("@3 = 1")
                    .And.Member("@4 = 1"));
        }

        [Test]
        public void MapMirrorsLeftAndRight([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);

            string map = _board.MapText.Replace("$-", "$$")
                .Replace("[]", "[[")
                .Replace("@1", "@@")
                .Replace("@2", "@@")
                .Replace("@3", "@@")
                .Replace("@4", "@@");

            Assert.That(map, Is.EqualTo(map.Reverse()));
        }

        [Test]
        public void MapMirrorsTopAndBottom([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);

            string map = _board.MapText.Replace("$-", "$$")
                .Replace("[]", "[[")
                .Replace("@1", "@@")
                .Replace("@2", "@@")
                .Replace("@3", "@@")
                .Replace("@4", "@@");

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
            GenerateMap(seed);

            int cells = _board.MapText.Length/2;
            double size = Math.Sqrt(cells);
            Assert.That(size,
                Is.EqualTo(Math.Floor(size))
                    .And.InRange(10, 28));
            Assert.That(size%2, Is.Not.EqualTo(1));
        }

        [Test]
        public void MinesAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            AssertTokenIsAlwaysBesideAnother("$-", _openTokens);
        }

        [Test]
        public void OpenPathBetweenLeftAndRightQuadrant([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);

            int half = _boardHelper.Size/2;
            for (int y = 1; y <= half; y++)
            {
                if (_openTokens.Any(t => t == _boardHelper[half, y])) Assert.Pass();
            }
            Assert.Fail("Missing path at top-left quardrent on right edge.\r\n{0}", _boardHelper);
        }

        [Test]
        public void OpenPathBetweenTopAndBottomQuadrant([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);

            string tokens = _board.MapText;
            var size = (int) Math.Sqrt(tokens.Length/2.0);

            // first line of tokens in bottom left quadrant
            string borderTokens = tokens.Substring(tokens.Length/2, size);

            Assert.That(_openTokens.Any(t => borderTokens.IndexOf(t, StringComparison.Ordinal) != -1), Is.True,
                "boardHelper does not have open path on quadrant border\r\n{0}",
                _boardHelper);
        }

        [Test]
        public void OpenPathIsNextToAnotherOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            AssertTokenIsAlwaysBesideAnother("  ", _openTokens);
        }

        [Test]
        public void PlayersAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            AssertTokenIsAlwaysBesideAnother("@1", _openTokens);
            AssertTokenIsAlwaysBesideAnother("@2", _openTokens);
            AssertTokenIsAlwaysBesideAnother("@3", _openTokens);
            AssertTokenIsAlwaysBesideAnother("@4", _openTokens);
        }

        [Test]
        public void TavernsAreNextToOpenPath([Random(MinSeed, MaxSeed, SeedCount)] int seed)
        {
            GenerateMap(seed);
            AssertTokenIsAlwaysBesideAnother("[]", _openTokens);
        }
    }
}
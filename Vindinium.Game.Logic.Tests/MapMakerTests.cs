using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class MapMakerTests
    {
        private static Grid NewMap()
        {
            return new Grid {MapText = MapMaker.GenerateMap()};
        }

        private static Dictionary<string, int> TokensOnNewMap()
        {
            Grid map = NewMap();
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

        [Test]
        public void MapDoesNotHaveUnexpectedTokens()
        {
            string[] actualTokens = TokensOnNewMap().Select(p => p.Key).ToArray();
            var expectedTokens = new[] {"@1", "@2", "@3", "@4", "$1", "$2", "$3", "$4", "[]", "$-", "##", "  "};
            Assert.That(actualTokens, Is.SubsetOf(expectedTokens));
        }

        [Test]
        public void MapHasEmptyMines()
        {
            string[] actualTokens = TokensOnNewMap().Select(p => p.Key).ToArray();
            Assert.That(actualTokens, Has.Member("$-"));
            Assert.That(actualTokens, Has.No.Member("$1"));
            Assert.That(actualTokens, Has.No.Member("$2"));
            Assert.That(actualTokens, Has.No.Member("$3"));
            Assert.That(actualTokens, Has.No.Member("$4"));
        }

        [Test]
        public void MapHasEmptyPath()
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap();
            Assert.That(actualTokens, Has.Some.Property("Key").EqualTo("  "));
        }

        [Test]
        public void MapHasFourTaverns()
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap();
            Assert.That(actualTokens["[]"], Is.EqualTo(4));
        }

        [Test]
        public void MapHasImpassibleWoods()
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap();
            Assert.That(actualTokens["##"], Is.GreaterThan(0));
        }

        [Test]
        public void MapHasPlayers()
        {
            Dictionary<string, int> actualTokens = TokensOnNewMap();
            Assert.That(actualTokens["@1"], Is.EqualTo(1));
            Assert.That(actualTokens["@2"], Is.EqualTo(1));
            Assert.That(actualTokens["@3"], Is.EqualTo(1));
            Assert.That(actualTokens["@4"], Is.EqualTo(1));
        }

        [Test]
        public void MapIsSymmetric()
        {
            string map = MapMaker.GenerateMap();

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
        public void MapTextIsExpectedLength()
        {
            string text = MapMaker.GenerateMap();
            int cells = text.Length/2;
            double size = Math.Sqrt(cells);
            Assert.That(size, Is.EqualTo(Math.Floor(size)));
            Assert.That(size, Is.AtLeast(10));
            Assert.That(size, Is.AtMost(28));
            Assert.That(size%2, Is.Not.EqualTo(1));
        }
    }
}
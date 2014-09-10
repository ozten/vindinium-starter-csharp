using NUnit.Framework;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GridTests
	{
		[Test]
		public void MapTest()
		{
			const string text = "  ##" +
			                    "@1[]";
			var map = new Grid {MapText = text};
			Assert.That(map.MapText, Is.EqualTo(text));
			Assert.That(map[1, 1], Is.EqualTo(text.Substring(0, 2)));
			Assert.That(map[2, 1], Is.EqualTo(text.Substring(2, 2)));
			Assert.That(map[1, 2], Is.EqualTo(text.Substring(4, 2)));
			Assert.That(map[2, 2], Is.EqualTo(text.Substring(6, 2)));
			for (int y = 1; y <= map.Size; y++)
			{
				for (int x = 1; x <= map.Size; x++)
				{
					Assert.That(map.PositionOf(map[x, y]), Is.EqualTo(new Pos {X = x, Y = y}));
				}
			}
		}
	}
}
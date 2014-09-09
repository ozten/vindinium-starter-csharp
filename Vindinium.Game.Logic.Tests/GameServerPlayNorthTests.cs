using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GameServerPlayNorthTests
	{
		[Test]
		public void CanMoveToEmptyArea()
		{
			var server = new GameServer();
			GameResponse response = server.Start("    @1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("@1      "));
		}
	}
}
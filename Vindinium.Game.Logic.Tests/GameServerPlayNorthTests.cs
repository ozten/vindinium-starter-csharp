using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GameServerPlayNorthTests
	{
		[Test]
		public void Steps()
		{
			var server = new GameServer();
			GameResponse response = server.Start("  ##@1##");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("@1##  ##"));
		}

		[Test]
		public void StepsOutOfMap()
		{
			var server = new GameServer();
			GameResponse response = server.Start("@1      ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("@1      "));
		}

		[Test]
		public void StepsOverGoldMine()
		{
			var server = new GameServer();
			GameResponse response = server.Start("$-  @1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
		}

		[Test]
		public void StepsOverGoldMineAndDies()
		{
			var server = new GameServer();
			GameResponse response = server.Start("$-  @1  ");
			for (int i = 0; i < 100; i++) server.Play(response.Token, Direction.Stay);
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$-  @1  "));
			Assert.That(response.Game.Players[0].Life, Is.EqualTo(100));
			Assert.That(response.Self.MineCount, Is.EqualTo(0));
		}

		[Test]
		public void StepsOverTree()
		{
			var server = new GameServer();
			GameResponse response = server.Start("##  @1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("##  @1  "));
		}

		[Test]
		public void ThirstDoesNotKill()
		{
			var server = new GameServer();
			GameResponse response = server.Start("  ##@1##");

			for (int i = 0; i < 100; i++)
				response = server.Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(1));
		}

		[Test]
		public void ThirstIsApplied()
		{
			var server = new GameServer();
			GameResponse response = server.Start("  ##@1##");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(99));
			Assert.That(response.Game.Players.First(p => p.Id == response.Self.Id).Life, Is.EqualTo(99));
		}
	}
}
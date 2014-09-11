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
		public void StepsIntoGoldMine()
		{
			var server = new GameServer();
			GameResponse response = server.Start("$-  @1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
		}

		[Test]
		public void StepsIntoGoldMineAndDies()
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
		public void StepsIntoGoldMineBelongsToSelf()
		{
			var server = new GameServer();
			GameResponse response = server.Start("$1  @1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
			Assert.That(response.Self.Life, Is.EqualTo(99));
		}

		[Test]
		public void StepsIntoGoldMineOfEnemy()
		{
			var server = new GameServer();
			GameResponse response = server.Start("$4@4@1  ");
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1@4@1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
			Assert.That(response.Self.Life, Is.EqualTo(79));
			Assert.That(response.Game.Players[3].MineCount, Is.EqualTo(0));
		}

		[Test]
		public void StepsIntoTavern()
		{
			var server = new GameServer();
			GameResponse response = server.Start("[]$1@1  ");
			for (int i = 0; i < 51; i++)
			{
				response = server.Play(response.Token, Direction.Stay);
			}
			int life = response.Self.Life;

			Assert.That(response.Self.Life, Is.LessThan(50));
			int gold = response.Self.Gold;
			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("[]$1@1  "));
			Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
			Assert.That(response.Self.Life, Is.EqualTo((life + 50) - 1));
		}

		[Test]
		public void StepsIntoTavernOverdrinking()
		{
			var server = new GameServer();
			GameResponse response = server.Start("[]$1@1  ");
			response = server.Play(response.Token, Direction.Stay);
			response = server.Play(response.Token, Direction.Stay);

			int gold = response.Self.Gold;

			response = server.Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(100));
			Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
		}

		[Test]
		public void StepsIntoTavernWithoutPayment()
		{
			var server = new GameServer();
			GameResponse response = server.Start("[]  @1  ");
			response = server.Play(response.Token, Direction.Stay);
			response = server.Play(response.Token, Direction.North);

			Assert.That(response.Self.Life, Is.EqualTo(98));
			Assert.That(response.Self.Gold, Is.EqualTo(0));
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
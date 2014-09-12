using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GameServerPlayNorthTests
	{
		#region Setup/Teardown

		[SetUp]
		public void BeforeEachTest()
		{
			_server = new GameServer();
		}

		#endregion

		private GameServer _server;

		private GameResponse Start(string mapText)
		{
			return _server.Start(mapText).JsonToObject<GameResponse>();
		}

		private GameResponse Play(string token, Direction direction)
		{
			return _server.Play(token, direction).JsonToObject<GameResponse>();
		}

		private void AssertPlayHasMapText(string token, Direction direction, string mapText)
		{
			var response = _server.Play(token, direction).JsonToObject<GameResponse>();
			Assert.That(response.Game.Board.MapText, Is.EqualTo(mapText));
		}

		[Test]
		public void Steps()
		{
			GameResponse response = Start("  ##@1##");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("@1##  ##"));
		}

		[Test]
		public void StepsIntoGoldMine()
		{
			GameResponse response = Start("$-  @1  ");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
		}

		[Test]
		public void StepsIntoGoldMineAndDies()
		{
			GameResponse response = Start("$-  @1  ");
			for (int i = 0; i < 100; i++) Play(response.Token, Direction.Stay);
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$-  @1  "));
			Assert.That(response.Game.Players[0].Life, Is.EqualTo(100));
			Assert.That(response.Self.MineCount, Is.EqualTo(0));
		}

		[Test]
		public void StepsIntoGoldMineBelongsToSelf()
		{
			GameResponse response = Start("$1  @1  ");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
			Assert.That(response.Self.Life, Is.EqualTo(99));
		}

		[Test]
		public void StepsIntoGoldMineOfEnemy()
		{
			GameResponse response = Start("$4@4@1  ");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("$1@4@1  "));
			Assert.That(response.Self.MineCount, Is.EqualTo(1));
			Assert.That(response.Self.Life, Is.EqualTo(79));
			Assert.That(response.Game.Players[3].MineCount, Is.EqualTo(0));
		}

		[Test]
		public void StepsIntoTavern()
		{
			GameResponse response = Start("[]$1@1  ");
			for (int i = 0; i < 51; i++)
			{
				response = Play(response.Token, Direction.Stay);
			}
			int life = response.Self.Life;

			Assert.That(response.Self.Life, Is.LessThan(50));
			int gold = response.Self.Gold;
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("[]$1@1  "));
			Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
			Assert.That(response.Self.Life, Is.EqualTo((life + 50) - 1));
		}

		[Test]
		public void StepsIntoTavernOverdrinking()
		{
			GameResponse response = Start("[]$1@1  ");
			response = Play(response.Token, Direction.Stay);
			response = Play(response.Token, Direction.Stay);

			int gold = response.Self.Gold;

			response = Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(100));
			Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
		}

		[Test]
		public void StepsIntoTavernWithoutPayment()
		{
			GameResponse response = Start("[]  @1  ");
			response = Play(response.Token, Direction.Stay);
			response = Play(response.Token, Direction.North);

			Assert.That(response.Self.Life, Is.EqualTo(98));
			Assert.That(response.Self.Gold, Is.EqualTo(0));
		}

		[Test]
		public void StepsOutOfMap()
		{
			GameResponse response = Start("@1      ");
			string token = response.Token;

			AssertPlayHasMapText(token, Direction.North, "@1      ");
			AssertPlayHasMapText(token, Direction.East, "  @1    ");
			AssertPlayHasMapText(token, Direction.East, "  @1    ");
			AssertPlayHasMapText(token, Direction.South, "      @1");
			AssertPlayHasMapText(token, Direction.South, "      @1");
			AssertPlayHasMapText(token, Direction.West, "    @1  ");
			AssertPlayHasMapText(token, Direction.West, "    @1  ");
			AssertPlayHasMapText(token, Direction.North, "@1      ");
			AssertPlayHasMapText(token, Direction.North, "@1      ");
		}

		[Test]
		public void StepsOverTree()
		{
			GameResponse response = Start("##  @1  ");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Game.Board.MapText, Is.EqualTo("##  @1  "));
		}

		[Test]
		public void ThirstDoesNotKill()
		{
			GameResponse response = Start("  ##@1##");

			for (int i = 0; i < 100; i++)
				response = Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(1));
		}

		[Test]
		public void ThirstIsApplied()
		{
			GameResponse response = Start("  ##@1##");
			response = Play(response.Token, Direction.North);
			Assert.That(response.Self.Life, Is.EqualTo(99));
			Assert.That(response.Game.Players.First(p => p.Id == response.Self.Id).Life, Is.EqualTo(99));
		}
	}
}
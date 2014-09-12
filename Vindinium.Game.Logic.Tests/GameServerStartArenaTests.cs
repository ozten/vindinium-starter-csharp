using System;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GameServerStartArenaTests
	{
		[TestFixtureSetUp]
		public void RunBeforeFirstTest()
		{
			var server = new GameServer();
			_gameResponse = server.Start(EnvironmentType.Arena).JsonToObject<GameResponse>();
			_game = _gameResponse.Game;

			Console.WriteLine(_game.Board);
		}

		private GameResponse _gameResponse;
		private Common.DataStructures.Game _game;

		[Test]
		public void AllPlayersHaveEloScore()
		{
			Assert.That(_game.Players, Has.All.Property("Elo").InRange(0, 3000));
		}
	}
}
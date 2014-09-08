using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
	[TestFixture]
	public class GameServerStartTests
	{
		#region Setup/Teardown

		[SetUp]
		public void Init()
		{
			var server = new GameServer();
			_gameResponse = server.Start();
			_game = _gameResponse.Game;
		}

		#endregion

		private GameResponse _gameResponse;
		private Common.DataStructures.Game _game;

		private List<string> GetTokens()
		{
			var tokens = new List<string>();
			int i = 0;
			while (i < _game.Board.MapText.Length)
			{
				string token = _game.Board.MapText.Substring(i, 2);
				if (!tokens.Contains(token))
					tokens.Add(token);
				i += 2;
			}
			return tokens;
		}

		[Test]
		public void AllPlayersHaveEloScore()
		{
			Assert.That(_game.Players, Has.All.Property("Elo").EqualTo(1200));
		}

		[Test]
		public void AllPlayersHaveFullLife()
		{
			Assert.That(_game.Players, Has.All.Property("Life").EqualTo(100));
		}

		[Test]
		public void AllPlayersHaveNames()
		{
			Assert.That(_game.Players, Has.All.Property("Name").Not.Null);
		}

		[Test]
		public void AllPlayersHaveNoGold()
		{
			Assert.That(_game.Players, Has.All.Property("Gold").EqualTo(0));
		}

		[Test]
		public void AllPlayersHaveNoMines()
		{
			Assert.That(_game.Players, Has.All.Property("MineCount").EqualTo(0));
		}

		[Test]
		public void AllPlayersHavePosition()
		{
			Assert.That(_game.Players, Has.All.Property("Pos"));
		}

		[Test]
		public void AllPlayersHaveSpawnPosition()
		{
			Assert.That(_game.Players, Has.All.Property("SpawnPos"));
		}

		[Test]
		public void AllPlayersHaveUniquePosition()
		{
			Assert.That(_game.Players.Select(h => h.Pos), Is.Unique);
		}

		[Test]
		public void EachHeroHasAnIdentity()
		{
			Assert.That(_game.Players, Has.All.Property("Id").InRange(1, 4));
			Assert.That(_game.Players.Select(h => h.Id), Is.Unique);
		}

		[Test]
		public void GameBoardHasMap()
		{
			Assert.That(_gameResponse.Game.Board.MapText, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void GameBoardHasSize()
		{
			Assert.That(_gameResponse.Game.Board.Size, Is.InRange(10, 28));
		}

		[Test]
		public void GameIdIsProvided()
		{
			Assert.That(_gameResponse.Game.Id, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void GameIsNotFinished()
		{
			Assert.That(_gameResponse.Game.Finished, Is.False);
		}

		[Test]
		public void HasAGameBoard()
		{
			Assert.That(_gameResponse.Game.Board, Is.Not.Null);
		}

		[Test]
		public void HeroesAreAtSpawn()
		{
			Assert.That(_game.Players.Select(h => h.Pos), Is.EqualTo(_game.Players.Select(h => h.SpawnPos)));
		}

		[Test]
		public void HeroesAreUnique()
		{
			Assert.That(_game.Players.Select(h => h.UserId), Is.Unique);
		}

		[Test]
		public void MapDoesNotHaveUnexpectedTokens()
		{
			List<string> actualTokens = GetTokens();
			var expectedTokens = new[] {"@1", "@2", "@3", "@4", "$1", "$2", "$3", "$4", "[]", "$-", "##", "  "};
			Assert.That(actualTokens, Is.SubsetOf(expectedTokens));
		}

		[Test]
		public void MapHasEmptyMines()
		{
			List<string> actualTokens = GetTokens();
			Assert.That(actualTokens, Has.Member("$-"));
			Assert.That(actualTokens, Has.No.Member("$1"));
			Assert.That(actualTokens, Has.No.Member("$2"));
			Assert.That(actualTokens, Has.No.Member("$3"));
			Assert.That(actualTokens, Has.No.Member("$4"));
		}

		[Test]
		public void MapHasEmptyPath()
		{
			List<string> actualTokens = GetTokens();
			Assert.That(actualTokens, Has.Member("  "));
		}

		[Test]
		public void MapHasImpassibleWoods()
		{
			List<string> actualTokens = GetTokens();
			Assert.That(actualTokens, Has.Member("##"));
		}

		[Test]
		public void MapHasPlayers()
		{
			List<string> actualTokens = GetTokens();
			Assert.That(actualTokens, Has.Member("@1"));
			Assert.That(actualTokens, Has.Member("@2"));
			Assert.That(actualTokens, Has.Member("@3"));
			Assert.That(actualTokens, Has.Member("@4"));
		}

		[Test]
		public void MapHasTaverns()
		{
			List<string> actualTokens = GetTokens();
			Assert.That(actualTokens, Has.Member("[]"));
		}

		[Test]
		public void MapTextIsExpectedLength()
		{
			Board board = _gameResponse.Game.Board;
			Assert.That(board.MapText.Length, Is.EqualTo(board.Size*board.Size*2));
		}

		[Test]
		public void MaxTurns()
		{
			Assert.That(_gameResponse.Game.MaxTurns, Is.GreaterThan(0));
		}

		[Test]
		public void NoPlayerHasCrashed()
		{
			Assert.That(_game.Players, Has.All.Property("Crashed").EqualTo(false));
		}

		[Test]
		public void PlayUrlContainsGameId()
		{
			Assert.That(_gameResponse.PlayUrl, Is.StringContaining(_gameResponse.Game.Id));
		}

		[Test]
		public void PlayUrlContainsToken()
		{
			Assert.That(_gameResponse.PlayUrl, Is.StringContaining(_gameResponse.Token));
		}

		[Test]
		public void PlayUrlFormat()
		{
			string url = string.Format("api/{0}/{1}/play", _gameResponse.Game.Id, _gameResponse.Token);
			Assert.That(_gameResponse.PlayUrl, Is.StringEnding(url));
		}

		[Test]
		public void SelfIsInPlayersList()
		{
			Assert.That(_game.Players, Has.Member(_gameResponse.Self));
		}

		[Test]
		public void SpawnPositionsAreUnique()
		{
			Assert.That(_game.Players.Select(h => h.SpawnPos), Is.Unique);
		}

		[Test]
		public void StartsWithFourPlayers()
		{
			Assert.That(_game.Players.Count, Is.EqualTo(4));
		}

		[Test]
		public void TokenIsProvided()
		{
			Assert.That(_gameResponse.Token, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void TurnIsEqualToSelfId()
		{
			Assert.That(_gameResponse.Game.Turn, Is.EqualTo(_gameResponse.Self.Id));
		}

		[Test]
		public void ViewUrlContainsGameId()
		{
			Assert.That(_gameResponse.ViewUrl, Is.StringContaining(_gameResponse.Game.Id));
		}

		[Test]
		public void ViewUrlDoesNotContainToken()
		{
			Assert.That(_gameResponse.ViewUrl, Is.Not.StringContaining(_gameResponse.Token));
		}

		[Test]
		public void ViewUrlFormat()
		{
			Assert.That(_gameResponse.ViewUrl, Is.StringEnding(_gameResponse.Game.Id));
		}
	}
}
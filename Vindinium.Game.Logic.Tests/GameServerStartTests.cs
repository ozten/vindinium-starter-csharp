using System;
using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Services;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerStartTests
    {
        [TestFixtureSetUp]
        public void RunBeforeFirstTest()
        {
            IGameServerProxy server = new GameServer();
            _gameResponse = server.StartTraining(300).JsonToObject<GameResponse>();
            _game = _gameResponse.Game;

            Console.WriteLine(_game.Board);
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;


        [Test]
        public void AllOtherPlayersWithoutUserId()
        {
            Assert.That(_game.Players, Has.Exactly(1).Property("UserId").EqualTo(_gameResponse.Self.UserId));
            Assert.That(_game.Players, Has.Exactly(3).Property("UserId").Null);
        }

        [Test]
        public void AllPlayersAtSpawnPolsition()
        {
            Assert.That(_game.Players.Select(h => h.Pos), Is.EqualTo(_game.Players.Select(h => h.SpawnPos)));
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
        public void BoardShowsPlayersAtPositions()
        {
            var grid = new Grid {MapText = _game.Board.MapText};
            foreach (Hero player in _game.Players)
            {
                string playerToken = string.Format("@{0}", player.Id);
                string mapToken = grid[player.Pos];
                Assert.That(mapToken, Is.EqualTo(playerToken));
            }
        }

        [Test]
        public void EachGameHasADifferentBoard()
        {
            Assert.Inconclusive("No guarantee that boards are always different");
            // TODO: Inject randomizer
            /*
            var server = new GameServer();
            var game1 = server.StartTraining(3000).JsonToObject<GameResponse>();
            var game2 = server.StartTraining(3000).JsonToObject<GameResponse>();
            Assert.That(game1.Game.Board, Is.Not.EqualTo(game2.Game.Board));
            */
        }

        [Test]
        public void EachGameHasADifferentGameId()
        {
            var server = new GameServer();
            var game1 = server.StartTraining(3000).JsonToObject<GameResponse>();
            var game2 = server.StartTraining(3000).JsonToObject<GameResponse>();
            Assert.That(game1.Game.Id, Is.Not.EqualTo(game2.Game.Id));
        }

        [Test]
        public void EachGameHasADifferentToken()
        {
            var server = new GameServer();
            var game1 = server.StartTraining(3000).JsonToObject<GameResponse>();
            var game2 = server.StartTraining(3000).JsonToObject<GameResponse>();
            Assert.That(game1.Token, Is.Not.EqualTo(game2.Token));
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
            Assert.That(_gameResponse.Game.Board.Size%2, Is.EqualTo(0), "Not even");
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
        public void PlayersFirstTurnIsRelatedToSelfId()
        {
            Assert.That(_gameResponse.Game.Turn, Is.EqualTo(_gameResponse.Self.Id - 1));
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
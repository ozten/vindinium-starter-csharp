using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Services;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerPlayTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
            _server = new GameServer();
        }

        private IGameServerProxy _server;

        private GameResponse Start(string mapText)
        {
            return _server.Start(mapText).JsonToObject<GameResponse>();
        }

        private GameResponse Play(string gameId, string token, Direction direction)
        {
            string text = _server.Play(gameId, token, direction);
            Assert.That(_server.ApiResponse.HasError, Is.False, _server.ApiResponse.ErrorMessage);
            Assert.That(_server.ApiResponse.ErrorMessage, Is.Null);
            Assert.That(_server.ApiResponse.Text, Is.EqualTo(text));
            return text.JsonToObject<GameResponse>();
        }

        private void AssertPlayError(string gameId, string token, Direction direction, string message)
        {
            string text = _server.Play(gameId, token, direction);
            Assert.That(_server.ApiResponse.HasError, Is.True);
            Assert.That(_server.ApiResponse.ErrorMessage, Is.EqualTo(message));
            Assert.That(text, Is.EqualTo(message));
        }

        private void AssertPlayHasMapText(string gameId, string token, Direction direction, string mapText)
        {
            var response = _server.Play(gameId, token, direction).JsonToObject<GameResponse>();
            Assert.That(response.Game.Board.MapText, Is.EqualTo(mapText));
        }

        [Test]
        public void KillEnemy()
        {
            GameResponse response = Start("@2$2@1$2");

            for (int i = 0; i < 5; i++)
                response = Play(response.Game.Id, response.Token, Direction.North);

            Hero player2 = response.Game.Players.First(p => p.Id == 2);
            Assert.That(player2.Life, Is.EqualTo(100));
            Assert.That(player2.MineCount, Is.EqualTo(0));
            Assert.That(response.Self.MineCount, Is.EqualTo(2));
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2$1@1$1"));
        }

        [Test]
        public void PlayWithWrongGame()
        {
            GameResponse response = Start("  ##@1##");
            AssertPlayError(response.Game.Id + "bad", response.Token, Direction.North, "Unable to find the game");
        }

        [Test]
        public void PlayWithWrongToken()
        {
            GameResponse response = Start("  ##@1##");
            AssertPlayError(response.Game.Id, response.Token + "bad", Direction.North,
                "Unable to find the token in your game");
        }

        [Test]
        public void SpawnOnDeath()
        {
            GameResponse response = Start("$-    @1");
            for (int i = 0; i < 80; i++)
                Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Self.Life, Is.EqualTo(19));
            response = Play(response.Game.Id, response.Token, Direction.North);

            Assert.That(response.Self.Life, Is.EqualTo(100));
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$-    @1"));
        }

        [Test]
        public void SpawnOnPlayer()
        {
            GameResponse response = Start("@1@2@3@4");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            _server.ChangeMap("@2@3@4@1"); // pretend players moved to each others spawns
            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2@3@4@1"));
            response = Play(response.Game.Id, response.Token, Direction.West);
            response = Play(response.Game.Id, response.Token, Direction.West);
            response = Play(response.Game.Id, response.Token, Direction.West);
            Assert.That(response.Game.Players.First(p => p.Id == 4).Life, Is.EqualTo(20));
            response = Play(response.Game.Id, response.Token, Direction.West);


            Assert.That(response.Game.Board.MapText, Is.EqualTo("@1@2@3@4"));
        }

        [Test]
        public void Steps()
        {
            GameResponse response = Start("  ##@1##");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@1##  ##"));
        }

        [Test]
        public void StepsIntoGoldMine()
        {
            GameResponse response = Start("$-  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
        }

        [Test]
        public void StepsIntoGoldMineAndDies()
        {
            GameResponse response = Start("$-  @1  ");
            for (int i = 0; i < 100; i++) Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$-  @1  "));
            Assert.That(response.Game.Players[0].Life, Is.EqualTo(100));
            Assert.That(response.Self.MineCount, Is.EqualTo(0));
        }

        [Test]
        public void StepsIntoGoldMineBelongsToSelf()
        {
            GameResponse response = Start("$1  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1  @1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
            Assert.That(response.Self.Life, Is.EqualTo(99));
        }

        [Test]
        public void StepsIntoGoldMineOfEnemy()
        {
            GameResponse response = Start("$4@4@1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("$1@4@1  "));
            Assert.That(response.Self.MineCount, Is.EqualTo(1));
            Assert.That(response.Self.Life, Is.EqualTo(79));
            Assert.That(response.Game.Players.First(p => p.Id == 4).MineCount, Is.EqualTo(0));
        }

        [Test]
        public void StepsIntoPlayerAndDamages()
        {
            GameResponse response = Start("@2  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("@2  @1  "));
            Assert.That(response.Game.Players.Where(p => p.Id == 2), Has.All.Property("Life").EqualTo(80));
            Assert.That(response.Self.Life, Is.EqualTo(99));
        }

        [Test]
        public void StepsIntoTavern()
        {
            GameResponse response = Start("[]$1@1  ");
            for (int i = 0; i < 51; i++)
            {
                response = Play(response.Game.Id, response.Token, Direction.Stay);
            }
            int life = response.Self.Life;

            Assert.That(response.Self.Life, Is.LessThan(50));
            int gold = response.Self.Gold;
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("[]$1@1  "));
            Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
            Assert.That(response.Self.Life, Is.EqualTo((life + 50) - 1));
        }

        [Test]
        public void StepsIntoTavernOverdrinking()
        {
            GameResponse response = Start("[]$1@1  ");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.Stay);

            int gold = response.Self.Gold;

            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(99));
            Assert.That(response.Self.Gold, Is.EqualTo((gold + response.Self.MineCount) - 2));
        }

        [Test]
        public void StepsIntoTavernWithoutPayment()
        {
            GameResponse response = Start("[]  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.Stay);
            response = Play(response.Game.Id, response.Token, Direction.North);

            Assert.That(response.Self.Life, Is.EqualTo(98));
            Assert.That(response.Self.Gold, Is.EqualTo(0));
        }

        [Test]
        public void StepsOutOfMap()
        {
            GameResponse response = Start("@1      ");
            string token = response.Token;
            string gameId = response.Game.Id;

            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
            AssertPlayHasMapText(gameId, token, Direction.East, "  @1    ");
            AssertPlayHasMapText(gameId, token, Direction.East, "  @1    ");
            AssertPlayHasMapText(gameId, token, Direction.South, "      @1");
            AssertPlayHasMapText(gameId, token, Direction.South, "      @1");
            AssertPlayHasMapText(gameId, token, Direction.West, "    @1  ");
            AssertPlayHasMapText(gameId, token, Direction.West, "    @1  ");
            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
            AssertPlayHasMapText(gameId, token, Direction.North, "@1      ");
        }

        [Test]
        public void StepsOverTree()
        {
            GameResponse response = Start("##  @1  ");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Game.Board.MapText, Is.EqualTo("##  @1  "));
        }

        [Test]
        public void ThirstDoesNotKill()
        {
            GameResponse response = Start("  ##@1##");

            for (int i = 0; i < 100; i++)
                response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(1));
        }

        [Test]
        public void ThirstIsApplied()
        {
            GameResponse response = Start("  ##@1##");
            response = Play(response.Game.Id, response.Token, Direction.North);
            Assert.That(response.Self.Life, Is.EqualTo(99));
            Assert.That(response.Game.Players.First(p => p.Id == response.Self.Id).Life, Is.EqualTo(99));
        }
    }
}
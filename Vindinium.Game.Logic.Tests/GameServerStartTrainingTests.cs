using System;
using System.Linq;
using NUnit.Framework;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic.Tests
{
    [TestFixture]
    public class GameServerStartTrainingTests
    {
        [TestFixtureSetUp]
        public void RunBeforeFirstTest()
        {
            var server = new GameServer();
            _gameResponse = server.Start(EnvironmentType.Training).JsonToObject<GameResponse>();
            _game = _gameResponse.Game;

            Console.WriteLine(_game.Board);
        }

        private GameResponse _gameResponse;
        private Common.DataStructures.Game _game;

        [Test]
        public void OnlySelfHasEloScoreInTraining()
        {
            Assert.That(_gameResponse.Self.Elo, Is.Not.Null);
            Assert.That(_game.Players.Except(new[] {_gameResponse.Self}), Has.All.Property("Elo").Null);
        }
    }
}
namespace Vindinium.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Vindinium.Messages;

    /// <summary>
    /// Test unmarshalling messages
    /// </summary>
    [TestFixture]
    public class MessagesTest
    {
        private string sampleText;
        private JObject jobject;

        /// <summary>
        /// Setup this instance.
        /// </summary>
        /// <remarks>
        /// Uses newtonsoft to create JObjects to test JObject constructors with.
        /// We are not testing whether this deserialization itself is being done correctly
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Vindinium.Tests.sample.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this.sampleText = reader.ReadToEnd();
            }

            this.jobject = JsonConvert.DeserializeObject<JObject>(this.sampleText); 
        }

        /// <summary>
        /// Checks our Equals method is defined properly. Note that this is implicitly also testing the Equals method of Hero
        /// </summary>
        [Test]
        public void GameStatesInstantiatedWithTheSameJObjectAreConsideredTheSame()
        {
            var left = new GameState(this.jobject);
            var right = new GameState(this.jobject);
            Assert.AreEqual(left, right);
        }

        /// <summary>
        /// Checks game state properties deserialize correctly.
        /// </summary>
        [Test]
        public void GameStatePropertiesAreCorrect()
        {
            var gs = new GameState(this.jobject);
            Assert.AreEqual(1100, gs.CurrentTurn);
            Assert.AreEqual(true, gs.Finished);
            Assert.AreEqual(1200, gs.MaxTurns);
            Assert.AreEqual(4, gs.Heroes.Count);
            Assert.AreEqual(new Uri("http://localhost:9000/api/s2xh3aig/lte0/play"), gs.PlayURL);
            Assert.AreEqual(new Uri("http://localhost:9000/s2xh3aig"), gs.ViewURL);
            Assert.AreEqual(18, gs.Dimensions.Item1);
            Assert.AreEqual(18, gs.Dimensions.Item2);
        }

        /// <summary>
        /// Checks invalid board positions cannot be used.
        /// </summary>
        [Test]
        public void CannotUseBoardTilesOutOfRange()
        {
            var gs = new GameState(this.jobject);
            Assert.Throws<ArgumentException>(() => gs.GetTile(-1, 0));
            Assert.Throws<ArgumentException>(() => gs.GetTile(0, -1));
            Assert.Throws<ArgumentException>(() => gs.GetTile(0, 18));
            Assert.Throws<ArgumentException>(() => gs.GetTile(18, 0));
        }

        /// <summary>
        /// Checks the tiles deserialize correctly.
        /// </summary>
        [Test]
        public void BoardTilesDeserializeCorrectly()
        {
            var gs = new GameState(this.jobject);
            Assert.AreEqual(Tile.ImpassableWood, gs.GetTile(0, 0));
            Assert.AreEqual(Tile.Free, gs.GetTile(0, 9));
            Assert.AreEqual(Tile.GoldMine4, gs.GetTile(3, 7));
            Assert.AreEqual(Tile.GoldMineNeutral, gs.GetTile(14, 7));
            Assert.AreEqual(Tile.Hero4, gs.GetTile(4, 8));
            Assert.AreEqual(gs.GetTile(6, 6), Tile.Tavern);
        }

        /// <summary>
        /// Checks if the JObject is invalid then ArgumentException is thrown.
        /// </summary>
        [Test]
        public void JObjectConstructorsRejectDodgyJObjects()
        {
            var j = new JObject();
            Assert.Throws<ArgumentException>(() => new GameState(j));
            Assert.Throws<ArgumentException>(() => new Hero(j));
            Assert.Throws<ArgumentNullException>(() => new GameState(null));
            Assert.Throws<ArgumentNullException>(() => new Hero(null));
        }

        /// <summary>
        /// Checks properties of heroes.
        /// </summary>
        [Test]
        public void HeroPropertiesAreCorrect()
        {
            var heroJObject = this.jobject["hero"];
            var hero = new Hero(heroJObject as JObject);
            Assert.AreEqual(4, hero.Id);
            Assert.AreEqual("vjousse", hero.Name);
            Assert.AreEqual(1200, hero.Elo);
            Assert.AreEqual(false, hero.Crashed);
            Assert.AreEqual(1078, hero.Gold);
            Assert.AreEqual(38, hero.Life);
            Assert.AreEqual(6, hero.MineCount);
            Assert.AreEqual(new Tuple<int, int>(4, 8), hero.Pos);
            Assert.AreEqual(new Tuple<int, int>(5, 11), hero.SpawnPos);
        }
    }
}
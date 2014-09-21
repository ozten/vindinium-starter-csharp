namespace Vindinium.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;

    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Vindinium.Bot;
    using Vindinium.Configuration;
    using Vindinium.Messages;
    using Vindinium.ServerStuff;

    [TestFixture]
    public class ServerStuffTest
    {
        [Test]
        public void CannotUseNullKeyOrServerUri()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerStuff(null, new Uri("http://vindinium.org"))); 
            Assert.Throws<ArgumentNullException>(() => new ServerStuff("qwerty", null)); 
        }

        [Test]
        public void CannotSubmitNullBot()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random);
            Assert.Throws<ArgumentNullException>(() => serverStuff.Submit(null));
        }

        [Test]
        public void IfServerReturnsBadJsonThenArgumentExceptionIsThrown()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random);
            serverStuff.Uploader = new DodgyUploader();
            Assert.Throws<ArgumentException>(() => serverStuff.Submit(new RandomBot()));
        }   

        /// <summary>
        /// Checks the submit method is called correctly.
        /// </summary>
        /// <remarks>This test is failing. I can see mockBot.Move(...) is being called twice with what looks like the right gameState so it's probably
        /// a fault in the Equals method of that class. Also this test is causing the web-browser to be opened, which should probably be disabled</remarks>
        [Test]
        public void CallsWithCorrectGameState()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random);
            serverStuff.Uploader = new DummyUploader(2);
            var sample = Util.GetResource("sample.json");
            var jo = JsonConvert.DeserializeObject<JObject>(sample);
            jo["game"]["finished"] = false;
            var gs = new GameState(jo);
            var mockBot = new Mock<IBot>();
            mockBot.Setup(x => x.Move(It.Is<GameState>(gs1 => gs.Equals(gs1)))).Returns(Direction.Stay);
            serverStuff.Submit(mockBot.Object);
            mockBot.Verify(x => x.Move(It.Is<GameState>(gs1 => gs.Equals(gs1))), Times.Exactly(2));
        }
    }

    internal class FakeBot : IBot
    {
        public Direction Move (GameState gameState)
        {
            throw new Exception(gameState.ToString());
        }

        public string Name {
            get {
                return("qwertyuiop");
            }
        }
    }

    internal sealed class DummyUploader : IUploader
    {
        private readonly string sampleText;
        private readonly string sampleTextFinished;
        private readonly int i;
        private readonly object uploadLock;
        private int j;

        // Instances of this class should not be shared between instances of ServerStuff.
        // Furthermore, instances of ServerStuff that use the DummyUploader should not
        // be used by multiple tests running concurrently

        // TODO can we do anything about such restrictions?
        public DummyUploader(int value)
        {
            this.i = value;
            this.sampleTextFinished = Util.GetResource("sample.json");
            var jo = JsonConvert.DeserializeObject<JObject>(sampleTextFinished);
            jo["game"]["finished"] = false;
            this.sampleText = jo.ToString();
            this.j = value;
            this.uploadLock = new object();
        }

        public string Upload(WebClient wc, Uri uri, string parameters)
        {
            lock (this.uploadLock)
            {
                if (this.j == 0)
                {
                    this.j = this.i;
                    return this.sampleTextFinished;
                }
                else
                {
                    this.j--;
                    return this.sampleText;
                }
            }
        }
    }

    internal sealed class DodgyUploader : IUploader
    {
        public string Upload(WebClient wc, Uri uri, string parameters)
        {
            return "{}";
        }
    }
}
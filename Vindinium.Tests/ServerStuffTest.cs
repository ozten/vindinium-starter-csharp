namespace Vindinium.Tests
{
    using System;
    using System.IO;
    using System.Linq;
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
        /// <summary>
        /// Checks ServerStuff cannot be instantiated with null key or server URI.
        /// </summary>
        [Test]
        public void CannotUseNullKeyOrServerUri()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerStuff(null, new Uri("http://vindinium.org"), false)); 
            Assert.Throws<ArgumentNullException>(() => new ServerStuff("qwerty", null, false)); 
        }

        /// <summary>
        /// Checks ServerStuff rejects null bots.
        /// </summary>
        [Test]
        public void CannotSubmitNullBot()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random, false);
            Assert.Throws<ArgumentNullException>(() => serverStuff.Submit(null));
        }

        /// <summary>
        /// Checks the server returns bad json then argument exception is thrown.
        /// </summary>
        /// <remarks>
        /// TODO this test verifies current behaviour. Is this behaviour really desirable though?
        /// It's not really the fault of the user of the dll, as <see cref="System.ArgumentException"/> might suggest,
        /// if the server does something bad.
        /// </remarks>
        [Test]
        public void IfServerReturnsBadJsonThenArgumentExceptionIsThrown()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random, false);
            serverStuff.Uploader = new DodgyUploader();
            Assert.Throws<ArgumentException>(() => serverStuff.Submit(new RandomBot()));
        }   

        /// <summary>
        /// Checks the submit method is called correctly.
        /// </summary>
        [Test]
        public void CallsWithCorrectGameState()
        {
            var serverStuff = new ServerStuff("not a real api key", 10, new Uri("http://vindinium.org"), Map.Random, false);
            serverStuff.Uploader = new DummyUploader(2);
            var sample = Util.GetResource("sample.json");
            var jo = JsonConvert.DeserializeObject<JObject>(sample);
            jo["game"]["finished"] = false;
            var gs = new GameState(jo);
            var mockBot = new Mock<IBot>(MockBehavior.Loose);
            mockBot.Setup(x => x.Move(It.Is<GameState>(y => true))).Returns(Direction.Stay);
            mockBot.Setup(x => x.Name).Returns("Mock bot");
            serverStuff.Submit(mockBot.Object);
            mockBot.Verify(x => x.Move(gs), Times.Exactly(2));
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
            var jo = JsonConvert.DeserializeObject<JObject>(this.sampleTextFinished);
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
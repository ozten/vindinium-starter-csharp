using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using log4net;

namespace Vindinium
{

    public sealed class ServerStuff
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerStuff));

        private string _key;
        private bool _trainingMode;
        private uint _turns;
        private string _map;
        private Uri _playURL;
        private Uri _serverURL;

        public GameState  GameState{ get; private set; }
        //if training mode is false, turns and map are ignored8
        public ServerStuff(string key, bool trainingMode, int turns, Uri serverURL, string map)
        {
            this._key = key;
            this._trainingMode = trainingMode;
            this._serverURL = serverURL;

            //the reaons im doing the if statement here is so that i dont have to do it later
            if (trainingMode)
            {
                this._turns = (uint)turns;
                this._map = map;
            }
        }
        //initializes a new game, its syncronised
        private void CreateGame()
        {
            this.GameState = new GameState();
            this.GameState.errored = false;

            string uri;
            
            if (_trainingMode)
            {
                uri = _serverURL + "api/training";
            }
            else
            {
                uri = _serverURL + "api/arena";
            }

            string myParameters = "key=" + _key;
            if (_trainingMode)
                myParameters += "&turns=" + _turns.ToString();
            if (_map != null)
                myParameters += "&map=" + _map;

            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string result = client.UploadString(uri, myParameters);
                    Deserialize(result);

                }
                catch (WebException exception)
                {
                    _logger.Error("Failed to contact ["+uri+"]");
                    _logger.Error("WebException ["+exception+"]");
                    this.GameState.errored = true;
                    if (exception.Response != null)
                    {
                        using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                        {
                            this.GameState.errorText = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        this.GameState.errored = true;
                        this.GameState.errorText = "The server is down";
                    }

                }
            }
        }
        //starts everything
        public void Submit(IBot bot)
        {
            if (bot != null)
            {
                _logger.Info("Running [" + bot.Name + "]");

                this.CreateGame();

                while (this.GameState.errored)
                {
                    _logger.Error(this.GameState.errorText);
                    Thread.Sleep(1000);
                    this.CreateGame();
                }

                //opens up a webpage so you can view the game, doing it async so we dont time out
                new Thread(() => {
                    using (System.Diagnostics.Process.Start(this.GameState.viewURL.ToString()))
                    {
                    }
                }).Start();
			
                while (!(this.GameState.finished))
                {
                    this.MoveHero(bot.Move(this.GameState).ToString());
                    _logger.Info("completed turn [" + this.GameState.currentTurn.ToString() + "]");
                    if (this.GameState.errored)
                    {
                        _logger.Error(this.GameState.errorText);
                        Thread.Sleep(1000);
                    }
                }


                _logger.Info("[" + bot.Name + "] finished");
            }
        }

        private Hero Dictionary2Hero(IDictonary<string, object> inp) = {
            var outp = new Hero();
            outp.
        }

        private void Deserialize(string json)
        {
            var gameResponse = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);


            _playURL = new Uri((string)gameResponse["playUrl"]);
            this.GameState.viewURL = new Uri((string)gameResponse["viewUrl"]);

            GameState.myHero = gameResponse["hero"];

            var game = (IDictionary<string, object>) gameResponse["game"];

            GameState.heroes = game["heroes"];

            GameState.currentTurn = (int)game["turn"];
            GameState.maxTurns = (int)game["maxTurns"];
            GameState.finished = (bool)game["finished"];
            var board = (IDictionary<string, object>)game["board"];
            GameState.CreateBoard((int)board["size"], (string)board["tiles"]);
        }

        private void MoveHero(string direction)
        {
            string myParameters = "key=" + _key + "&dir=" + direction;
            
            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                try
                {
                    string result = client.UploadString(_playURL, myParameters);
                    Deserialize(result);
                }
                catch (WebException exception)
                {
                    GameState.errored = true;
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        GameState.errorText = reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
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
using Newtonsoft.Json.Linq;
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
        private Uri _serverURL;

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
        private GameState CreateGame()
        {

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
                    return new GameState(result);

                }
                catch (WebException exception)
                {
                    _logger.Error("Failed to contact ["+uri+"]");
                    _logger.Error("WebException ["+exception+"]", exception);

                    return new GameState(exception);
                }

            }
        }
        //starts everything
        public void Submit(IBot bot)
        {
            if (bot != null)
            {
                _logger.Info("Running [" + bot.Name + "]");

                // TODO destructive assignation to method-local variable gameState
                var gameState = this.CreateGame();

                while (gameState.Errored)
                {
                    _logger.Error(gameState.ErrorText);
                    Thread.Sleep(1000);
                    gameState = this.CreateGame();
                }

                //opens up a webpage so you can view the game, doing it async so we dont time out
                new Thread(() => {
                    using (System.Diagnostics.Process.Start(gameState.ViewURL.ToString()))
                    {
                    }
                }).Start();
			
                while (!(gameState.Finished))
                {
                    gameState = this.MoveHero(bot.Move(gameState).ToString(), gameState.PlayURL);
                    _logger.Info("completed turn [" + gameState.CurrentTurn.ToString() + "]");
                    if (gameState.Errored)
                    {
                        _logger.Error(gameState.ErrorText);
                        Thread.Sleep(1000);
                    }
                }


                _logger.Info("[" + bot.Name + "] finished");
            }
        }








        private GameState MoveHero(string direction, Uri playURL)
        {
            string myParameters = "key=" + _key + "&dir=" + direction;
            
            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                try
                {
                    string result = client.UploadString(playURL, myParameters);
                    return new GameState(result);
                }
                catch (WebException exception)
                {
                    _logger.Error("Failed to contact ["+playURL+"]");
                    _logger.Error("WebException ["+exception+"]", exception);

                    return new GameState(exception);
                }
            }
        }
    }
}
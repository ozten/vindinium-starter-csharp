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

    /// <summary>
    /// Represents connection to Vindinium Server.
    /// </summary>
    public sealed class ServerStuff
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff"/> class.
        /// </summary>
        /// <remarks>If training mode is false, turns and map are ignored.</remarks>
        /// <param name="key">Your API key that you got from the vindinium server.</param>
        /// <param name="trainingMode">If set to <c>true</c> training mode; otherwise arena mode.</param>
        /// <param name="turns">The number of turns that the game should last.</param>
        /// <param name="serverURL">The URL of the server.</param>
        /// <param name="map">The Vindinium map to use.</param>
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

        /// <summary>
        /// Runs the bot against the server in question.
        /// </summary>
        /// <param name="bot">The bot to run.</param>
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

                while (!(gameState.Finished == false))
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

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerStuff));
        private string _key;
        private bool _trainingMode;
        private uint _turns;
        private string _map;
        private Uri _serverURL;


        //initializes a new game, its syncronised
        private GameState CreateGame()
        {

            Uri uri = new Uri(_serverURL + ( _trainingMode ? "api/training" : "api/arena"));

            string myParameters = "key=" + _key;
            if (_trainingMode)
                myParameters += "&turns=" + _turns.ToString();
            if (_map != null)
                myParameters += "&map=" + _map;

            return Upload(uri, myParameters);
        }

        private GameState Upload(Uri uri, string parameters)
        {
            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string result = client.UploadString(uri, parameters);
                    var gameResponse = JsonConvert.DeserializeObject<JObject>(result);
                    return new GameState(gameResponse);

                }
                catch (WebException exception)
                {
                    _logger.Error("Failed to contact ["+uri+"]");
                    _logger.Error("WebException ["+exception+"]", exception);

                    return new GameState(exception);
                }

            }
        }


        private GameState MoveHero(string direction, Uri playURL)
        {
            string myParameters = "key=" + _key + "&dir=" + direction;
            return Upload(playURL, myParameters);
        }
    }
}
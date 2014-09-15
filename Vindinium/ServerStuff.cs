using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            this._serverURL = serverURL ?? new Uri("http://vindinium.org");

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

                var gameState = RetryUntilSuccessful(this.CreateGame, 1000);

                //opens up a webpage so you can view the game, doing it async so we dont time out
                // TODO should we really use a TaskFactory or TaskScheduler here?
                // would we gain anything by doing so?
                new Thread(() =>
                {
                    using (System.Diagnostics.Process.Start(gameState.ViewURL.ToString()))
                    {
                    }
                }).Start();

                while (!gameState.Finished)
                {
                    Func<IEither<GameState, ErrorState>> f = () => 
                        this.MoveHero(bot.Move(gameState).ToString(), gameState.PlayURL);
                    
                    gameState = RetryUntilSuccessful(f, 1000);

                    _logger.Info("completed turn [" + gameState.CurrentTurn.ToString() + "]");

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
        private IEither<GameState, ErrorState> CreateGame()
        {

            Uri uri = new Uri(_serverURL + (_trainingMode ? "api/training" : "api/arena"));

            string myParameters = "key=" + _key;
            if (_trainingMode)
                myParameters += "&turns=" + _turns.ToString();
            if (_map != null)
                myParameters += "&map=" + _map;

            return Upload(uri, myParameters);
        }

        private IEither<GameState, ErrorState> Upload(Uri uri, string parameters)
        {
            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string result = client.UploadString(uri, parameters);
                    var gameResponse = JsonConvert.DeserializeObject<JObject>(result);
                    return new Left<GameState>(new GameState(gameResponse));
                }
                catch (WebException exception)
                {
                    _logger.Error("Failed to contact [" + uri + "]");
                    _logger.Error("WebException [" + exception + "]", exception);

                    return new Right<ErrorState>(new ErrorState(exception));
                }

            }
        }


        private IEither<GameState, ErrorState> MoveHero(string direction, Uri playURL)
        {
            string myParameters = "key=" + _key + "&dir=" + direction;
            return Upload(playURL, myParameters);
        }

        internal static T RetryUntilSuccessful<T,U>(Func<IEither<T, U>> f, int wait) where T : class where U : class
        {
            var either = f().Value;
            var u = either as U;
            if (u != null)
            {
                _logger.Error("Error value [" + u.ToString() + "]");
                Thread.Sleep(wait);
                // TODO exponential backoff
                return RetryUntilSuccessful<T, U>(f, wait);
            }
            else
            {
                return either as T;
            }
        }
    }

    internal sealed class ErrorState
    {
        internal bool Errored { get; private set; }

        internal string ErrorText { get; private set; }

        internal ErrorState(WebException exception)
        {
            this.Errored = true;
            if (exception.Response != null)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    this.ErrorText = reader.ReadToEnd();
                }
            }
            else
            {
                this.ErrorText = "The server is down";
            }
        }
    }
}

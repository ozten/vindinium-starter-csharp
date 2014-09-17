namespace Vindinium
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents connection to Vindinium Server.
    /// </summary>
    public sealed class ServerStuff
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerStuff));
        private string _key;
        private string _trainingMode;
        private uint _turns;
        private string _map;
        private Uri _serverURL;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff"/> class.
        /// </summary>
        /// <remarks>This constructor uses a config file to initialize the connection.</remarks>
        public ServerStuff()
        {
            var config = (Vindinium.ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            this.Setup(config.Key, config.Mode, config.Turns, config.ServerUrl, config.Map);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff"/> class.
        /// </summary>
        /// <remarks>If trainingMode is Arena, turns and map are ignored.
        /// use this constructor if you don't want to use a config file to configure your connection.</remarks>
        /// <param name="key">Your API key that you got from the vindinium server.</param>
        /// <param name="trainingMode">The mode to run the bot in.</param>
        /// <param name="turns">The number of turns that the game should last.</param>
        /// <param name="serverURL">The URL of the server.</param>
        /// <param name="map">The Vindinium map to use.</param>
        public ServerStuff(string key, Mode trainingMode, int turns, Uri serverURL, Map map)
        {
            this.Setup(key, trainingMode, turns, serverURL, map);
        }

        /// <summary>
        /// Creates an instance of ServerStuff from a config file and uses it to run a bot specified in the config file.
        /// </summary>
        public static void Start()
        {
            var config = (Vindinium.ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            var serverStuff = new ServerStuff();
            if (config != null)
            {
                var botTypeS = config.Bot;
                if (botTypeS != null)
                {
                    var botType = Type.GetType(botTypeS);
                    if (botType == null)
                    {
                        _logger.Error("Couldn't find type [" + botTypeS + "], did you remember to specify the assembly too?");
                    }
                    else
                    {
                        try
                        {
                            var maybeBot = Activator.CreateInstance(botType);
                            var bot = maybeBot as IBot;
                            if (bot == null)
                            {
                                _logger.Error("[" + botType + "] does not seem to implement IBot");
                            }
                            else
                            {
                                serverStuff.Submit(bot);
                            }
                        }
                        catch (MissingMethodException e)
                        {
                            _logger.Error("The specified class doesn't seem to have a zero-argument constructor", e);
                        }
                    }
                }
            }
            else
            {
                _logger.Error("Can't find the config");
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

                // opens up a webpage so you can view the game, doing it async so we dont time out
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
            
        internal static T RetryUntilSuccessful<T, U>(Func<IEither<T, U>> f, int wait) where T : class where U : class
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

        // initializes a new game, its syncronised
        private IEither<GameState, ErrorState> CreateGame()
        {
            Uri uri = new Uri(this._serverURL + "api/" + this._trainingMode);

            string myParameters = "key=" + this._key;
            if (this._trainingMode == "training")
            {
                myParameters += "&turns=" + this._turns.ToString();
                if (this._map != null)
                {
                    myParameters += "&map=" + this._map;
                }
            }

            return this.Upload(uri, myParameters);
        }

        private IEither<GameState, ErrorState> Upload(Uri uri, string parameters)
        {
            _logger.Debug("URI: [" + uri + "]");
            _logger.Debug("Params: [" + parameters + "]");

            // make the request
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
            string myParameters = "key=" + this._key + "&dir=" + direction;
            return this.Upload(playURL, myParameters);
        }

        private void Setup(string key, Mode trainingMode, int turns, Uri serverURL, Map map)
        {
            this._key = key;
            this._trainingMode = trainingMode.ToString().ToLower(CultureInfo.InvariantCulture);
            this._serverURL = serverURL ?? new Uri("http://vindinium.org");

            // the reaons im doing the if statement here is so that i dont have to do it later
            if (trainingMode == Mode.Training)
            {
                this._turns = (uint)turns;
                if (map != Map.Random)
                {
                    this._map = map.ToString().ToLower(CultureInfo.InvariantCulture);
                }
            }
        }
    }

    internal sealed class ErrorState
    {
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

        internal bool Errored { get; private set; }

        internal string ErrorText { get; private set; }
    }   
}

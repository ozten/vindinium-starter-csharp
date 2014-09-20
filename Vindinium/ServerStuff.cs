namespace Vindinium.ServerStuff
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
    using Vindinium.Bot;
    using Vindinium.Configuration;
    using Vindinium.Messages;
    using Vindinium.Util;

    /// <summary>
    /// Represents connection to Vindinium Server.
    /// </summary>
    public abstract class AbstractServerStuff
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerStuff));
        private string key;
        private string trainingMode;
        private uint turns;
        private string map;
        private Uri serverURL;

        /// <summary>
        /// Creates an instance of ServerStuff from a config file and uses it to run a bot specified in the config file.
        /// </summary>
        public static void Start()
        {
            var config = (ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            var serverStuff = new ServerStuff();
            if (config != null)
            {
                var botTypeS = config.Bot;
                if (botTypeS != null)
                {
                    var botType = Type.GetType(botTypeS);
                    if (botType == null)
                    {
                        Logger.Error("Couldn't find type [" + botTypeS + "], did you remember to specify the assembly too?");
                    }
                    else
                    {
                        try
                        {
                            var maybeBot = Activator.CreateInstance(botType);
                            var bot = maybeBot as IBot;
                            if (bot == null)
                            {
                                Logger.Error("[" + botType + "] does not seem to implement IBot");
                            }
                            else
                            {
                                serverStuff.Submit(bot);
                            }
                        }
                        catch (MissingMethodException e)
                        {
                            Logger.Error("The specified class doesn't seem to have a zero-argument constructor", e);
                        }
                    }
                }
            }
            else
            {
                Logger.Error("Can't find the config");
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
                Logger.Info("Running [" + bot.Name + "]");

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

                    Logger.Info("completed turn [" + gameState.CurrentTurn.ToString() + "]");
                }

                Logger.Info("[" + bot.Name + "] finished");
            }
        }

        internal static T RetryUntilSuccessful<T, U>(Func<IEither<T, U>> f, int wait)
        {
            var either = f().Value;
            if (typeof(U).IsAssignableFrom(either.GetType()))
            {
                var u = (U)either;
                Logger.Error("Error value [" + u.ToString() + "]");
                Thread.Sleep(wait);

                // TODO exponential backoff
                return RetryUntilSuccessful<T, U>(f, wait);
            }
            else
            {
                return (T)either;
            }
        }

        /// <summary>
        /// Describes how to send data to the server.
        /// </summary>
        /// <param name="webClient">Web client.</param>
        /// <param name="uri">URI.</param>
        /// <param name="parameters">Parameters.</param>
        protected abstract string Upload(WebClient webClient, Uri uri, string parameters);

        /// <summary>
        /// Sets up the current instance.
        /// </summary>
        /// <remarks>Intended to be called in the constructor.</remarks>
        /// <param name="currentKey">The API key.</param>
        /// <param name="currentTrainingMode">Mode to use (training or arena).</param>
        /// <param name="currentTurns">Number of turns.</param>
        /// <param name="currentServerUrl">URL of vindinium server.</param>
        /// <param name="currentMap">Map to use.</param>
        /// <remarks><c>currentMap</c> and <c>currentTurns</c> will be ignored in Arena mode.
        /// This method is intended to be called from the constructor.</remarks>
        protected void Setup(string currentKey, Mode currentTrainingMode, int currentTurns, Uri currentServerUrl, Map currentMap)
        {
            this.key = currentKey;
            this.trainingMode = currentTrainingMode.ToString().ToLower(CultureInfo.InvariantCulture);
            this.serverURL = currentServerUrl ?? new Uri("http://vindinium.org");

            // the reaons im doing the if statement here is so that i dont have to do it later
            if (currentTrainingMode == Mode.Training)
            {
                this.turns = (uint)currentTurns;
                if (currentMap != Map.Random)
                {
                    this.map = currentMap.ToString().ToLower(CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Configures the connection from a config file.
        /// </summary>
        /// <remarks>
        /// Intended to be called in the constructor.
        /// </remarks>
        protected void ConfigureFromFile()
        {
            var config = (ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            this.Setup(config.Key, config.Mode, config.Turns, config.ServerUrl, config.Map);
        }

        // initializes a new game, its syncronised
        private IEither<GameState, ErrorState> CreateGame()
        {
            Uri uri = new Uri(this.serverURL + "api/" + this.trainingMode);

            string myParameters = "key=" + this.key;
            if (this.trainingMode == "training")
            {
                myParameters += "&turns=" + this.turns.ToString();
                if (this.map != null)
                {
                    myParameters += "&map=" + this.map;
                }
            }

            return this.Upload(uri, myParameters);
        }

        private IEither<GameState, ErrorState> Upload(Uri uri, string parameters)
        {
            Logger.Debug("URI: [" + uri + "]");
            Logger.Debug("Params: [" + parameters + "]");

            // make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string result = this.Upload(client, uri, parameters);
                    var gameResponse = JsonConvert.DeserializeObject<JObject>(result);
                    return new Left<GameState, ErrorState>(new GameState(gameResponse));
                }
                catch (WebException exception)
                {
                    Logger.Error("Failed to contact [" + uri + "]");
                    Logger.Error("WebException [" + exception + "]", exception);

                    return new Right<GameState, ErrorState>(new ErrorState(exception));
                }
            }
        }

        private IEither<GameState, ErrorState> MoveHero(string direction, Uri playURL)
        {
            string myParameters = "key=" + this.key + "&dir=" + direction;
            return this.Upload(playURL, myParameters);
        }
    }

    /// <summary>
    /// Server stuff to be used.
    /// </summary>
    public sealed class ServerStuff : AbstractServerStuff
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff.ServerStuff"/> class.
        /// </summary>
        /// <remarks>Use this constructor to configure the class from a config file.</remarks>
        public ServerStuff()
        {
            this.ConfigureFromFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff.ServerStuff"/> class.
        /// </summary>
        /// <param name="currentKey">The API key.</param>
        /// <param name="currentTrainingMode">Mode to use (training or arena).</param>
        /// <param name="currentTurns">Number of turns.</param>
        /// <param name="currentServerUrl">URL of vindinium server.</param>
        /// <param name="currentMap">Map to use.</param>
        /// <remarks>Use this constructor if you don't want to configure the class from a config file.</remarks>
        public ServerStuff(string currentKey, Mode currentTrainingMode, int currentTurns, Uri currentServerUrl, Map currentMap)
        {
            this.Setup(currentKey, currentTrainingMode, currentTurns, currentServerUrl, currentMap);
        }

        /// <summary>
        /// Describes how to send data to the server.
        /// </summary>
        /// <param name="webClient">Web client.</param>
        /// <param name="uri">URI.</param>
        /// <param name="parameters">Parameters.</param>
        protected override string Upload(WebClient webClient, Uri uri, string parameters)
        {
            if (webClient != null)
            {
                return webClient.UploadString(uri, parameters);
            }
            else
            {
                throw new ArgumentNullException("webClient");
            }
        }
    }

    internal sealed class ErrorState
    {
        internal ErrorState(WebException exception)
        {
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

        internal string ErrorText { get; private set; }
    }
}

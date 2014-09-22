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

    internal interface IUploader
    {
        string Upload(WebClient wc, Uri uri, string parameters);
    }

    /// <summary>
    /// Represents connection to Vindinium Server.
    /// </summary>
    public sealed class ServerStuff
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerStuff));
        private string key;
        private string trainingMode;
        private uint turns;
        private string map;
        private Uri serverURL;
        private bool openWebBrowser;

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
        /// <param name="currentServerUrl">URL of vindinium server.</param>
        /// <param name="currentOpenWebBrowser">Whether or not to open a web browser to view the game.</param> 
        /// <remarks>Use this constructor if you don't want to configure
        /// the class from a config file and want to run in arena mode.</remarks>
        public ServerStuff(string currentKey, Uri currentServerUrl, bool currentOpenWebBrowser)
        {
            if (currentKey == null)
            {
                throw new ArgumentNullException("currentKey");
            }
            else if (currentServerUrl == null)
            {
                throw new ArgumentNullException("currentServerUrl");
            }
            else
            {
                this.Setup(currentKey, Mode.Arena, 0, currentServerUrl, Map.Random, currentOpenWebBrowser);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vindinium.ServerStuff.ServerStuff"/> class.
        /// </summary>
        /// <param name="currentKey">The API key.</param>
        /// <param name="currentTurns">Number of turns.</param>
        /// <param name="currentServerUrl">URL of vindinium server.</param>
        /// <param name="currentMap">Map to use.</param>
        /// <param name="currentOpenWebBrowser">Whether or not to open a web browser to view the game.</param> 
        /// <remarks>Use this constructor if you don't want to configure
        /// the class from a config file and want to run in training mode.</remarks>
        public ServerStuff(string currentKey, int currentTurns, Uri currentServerUrl, Map currentMap, bool currentOpenWebBrowser)
        {
            if (currentServerUrl == null)
            {
                throw new ArgumentNullException("currentServerUrl");
            }
            else if (currentKey == null)
            {
                throw new ArgumentNullException("currentKey");
            }
            else 
            {
                this.Setup(currentKey, Mode.Training, currentTurns, currentServerUrl, currentMap, currentOpenWebBrowser);
            }
        }

        internal IUploader Uploader { get; set; }

        /// <summary>
        /// Creates an instance of ServerStuff from a config file and uses it to run a bot specified in the config file.
        /// </summary>
        public static void Start()
        {
            var serverStuff = new ServerStuff();
            serverStuff.Submit();
        }

        /// <summary>
        /// Submits a bot specified in a config file.
        /// </summary>
        public void Submit()
        {
            var config = (ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            if (config != null)
            {
                var botTypeS = config.Bot;
                if (botTypeS != null)
                {
                    var botType = Type.GetType(botTypeS);
                    if (ReferenceEquals(botType, null))
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
                                this.Submit(bot);
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

                var gameState = this.CreateGame();

                // opens up a webpage so you can view the game, doing it async so we dont time out
                // TODO should we really use a TaskFactory or TaskScheduler here?
                // would we gain anything by doing so?
                if (this.openWebBrowser)
                {
                    new Thread(() =>
                    {
                        var g = gameState.Value as GameState;
                        if (g != null)
                        {
                            using (System.Diagnostics.Process.Start(g.ViewURL.ToString()))
                            {
                            }
                        }
                    }).Start();
                }

                this.MoveHero(gameState, bot);

                Logger.Info("[" + bot.Name + "] finished");
            }
            else
            {
                throw new ArgumentNullException("bot");
            }
        }

        private void Setup(string currentKey, Mode currentTrainingMode, int currentTurns, Uri currentServerUrl, Map currentMap, bool currentOpenWebBrowser)
        {
            this.Uploader = new Uploader();
            this.key = currentKey;
            this.trainingMode = currentTrainingMode.ToString().ToLower(CultureInfo.InvariantCulture);
            this.serverURL = currentServerUrl ?? new Uri("http://vindinium.org");
            this.openWebBrowser = currentOpenWebBrowser;

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

        private void ConfigureFromFile()
        {
            var config = (ConfigurationSection)System.Configuration.ConfigurationManager.GetSection("Vindinium");
            this.Setup(config.Key, config.Mode, config.Turns, config.ServerUrl, config.Map, config.OpenWebBrowser);
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
                    string result = this.Uploader.Upload(client, uri, parameters);
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

        private void MoveHero(IEither<GameState, ErrorState> either, IBot bot)
        {
            var gameStateMaybe = either.Value;
            var gameState = gameStateMaybe as GameState;
            if (gameState == null)
            {
                Logger.Error("Something has gone wrong, cannot continue.");
                var errorState = gameStateMaybe as ErrorState;
                if (errorState == null)
                {
                    Logger.Error("Don't know what happened.");
                }
                else
                {
                    Logger.Error(errorState.ErrorText);
                }
            }
            else
            {
                if (!gameState.Finished)
                {
                    var direction = bot.Move(gameState);
                    string myParameters = string.Format(CultureInfo.InvariantCulture, "key={0}&dir={1}", this.key, direction.ToString());
                    this.MoveHero(this.Upload(gameState.PlayURL, myParameters), bot);
                }
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

    internal sealed class Uploader : IUploader
    {
        public string Upload(WebClient wc, Uri uri, string parameters)
        {
            return wc.UploadString(uri, parameters);
        }
    }
}

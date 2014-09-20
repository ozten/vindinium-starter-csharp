namespace Vindinium.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// The mode that the bot is running in (Training/Arena).
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Training Mode
        /// </summary>
        Training = 0,

        /// <summary>
        /// Arena Mode
        /// </summary>
        Arena = 1
    }

    /// <summary>
    /// The map to use in training mode.
    /// </summary>
    public enum Map
    {
        /// <summary>
        /// Choose a random map
        /// </summary>
        Random = 0,

        /// <summary>
        /// Use the first map
        /// </summary>
        M1,

        /// <summary>
        /// Use the second map
        /// </summary>
        M2,

        /// <summary>
        /// Use the third map
        /// </summary>
        M3,

        /// <summary>
        /// Use the fourth map
        /// </summary>
        M4,

        /// <summary>
        /// Use the fifth map
        /// </summary>
        M5,

        /// <summary>
        /// Use the sixth map
        /// </summary>
        M6
    }

    /// <summary>
    /// Configuration for connecting to the vindinium server.
    /// </summary>
    public class ConfigurationSection : System.Configuration.ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the API key you were given by the vindinium server.
        /// </summary>
        /// <value>The key.</value>
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        /// <summary>
        /// Gets or sets the mode that the bot is running in.
        /// </summary>
        /// <value>The mode.</value>
        [ConfigurationProperty("mode", IsRequired = true)]
        public Mode Mode
        {
            get { return (Mode)this["mode"]; }
            set { this["mode"] = value; }
        }

        /// <summary>
        /// Gets or sets the number of turns.
        /// </summary>
        /// <remarks>This will be ignored when the mode is "Arena".</remarks>
        /// <value>The turns.</value>
        [ConfigurationProperty("turns", DefaultValue = "10", IsRequired = false)]
        public int Turns
        {
            get { return (int)this["turns"]; }
            set { this["turns"] = value; }
         }

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        /// <remarks>Defaults to http://vindinium.org</remarks>
        /// <value>The server URL.</value>
        [ConfigurationProperty("serverUrl", DefaultValue = "http://vindinium.org", IsRequired = false)]
        public Uri ServerUrl
        {
            get { return (Uri)this["serverUrl"]; }
            set { this["serverUrl"] = value; }
        }

        /// <summary>
        /// Gets or sets the map to be used in training mode.
        /// </summary>
        /// <remarks>If not specified, a random map will be chosen; this property
        /// is not relevant in Arena mode.</remarks>
        /// <value>The map.</value>
        [ConfigurationProperty("map", DefaultValue = "Random", IsRequired = false)]
        public Map Map 
        {
            get { return (Map)this["map"]; }
            set { this["map"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the bot to use, represented as a string.
        /// </summary>
        /// <remarks>The type in question must extend IBot and have a zero-argument
        /// constructor. If specified the bot will be created and run
        /// when the ServerStuff.Start() is called. If not specified then
        /// ServerStuff can be constructed with its zero-argument constructor and
        /// a bot can be submitted later.</remarks>
        /// <value>The bot.</value>
        [ConfigurationProperty("bot", IsRequired = false)]
        public string Bot
        {
            get { return (string)this["bot"]; }
            set { this["bot"] = value; }
        }
    }
}
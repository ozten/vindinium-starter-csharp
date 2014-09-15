using System;
using System.Configuration;
using System.Collections.Generic;
using log4net.Config;

namespace Vindinium
{
    static class Client
    {
        static void Main()
        {
            XmlConfigurator.Configure();
            var settings = ConfigurationManager.AppSettings;  
            var maybeUri = settings["uri"];
            var maybeTurns = settings["turns"];
            int turnsTmp;
            // an out parameter! 
            var turns = int.TryParse(maybeTurns, out turnsTmp) ? turnsTmp : 10;
            ServerStuff serverStuff = new ServerStuff(settings["key"], settings["mode"] != "arena",
                turns, maybeUri == null ? null : new Uri(maybeUri), settings["map"]);

            //create the random bot, replace this with your own bot
            RandomBot bot = new RandomBot();

            //now kick it all off by running the bot.
            serverStuff.Submit(bot);

            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}

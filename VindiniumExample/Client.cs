using System;
using System.Configuration;
using System.Collections.Generic;
using log4net.Config;

namespace Vindinium
{
    static class Client
    {
       
        /**
         * Launch client.
         * @param args args[0] Private key
         * @param args args[1] [training|arena]
         * @param args args[2] number of turns
         * @param args args[3] HTTP URL of Vindinium server (optional)
         */
        static void Main(string[] args)
        {
            //create the server stuff, when not in training mode, it doesnt matter
            XmlConfigurator.Configure();
            var settings = ConfigurationManager.AppSettings;  
            var maybeUri = settings["uri"];
            ServerStuff serverStuff = new ServerStuff(settings["key"], settings["mode"] != "arena",
                int.Parse(settings["turns"]), maybeUri == null ? null : new Uri(maybeUri), settings["map"]);

            //create the random bot, replace this with your own bot
            RandomBot bot = new RandomBot();

            //now kick it all off by running the bot.
            serverStuff.Submit(bot);

            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}

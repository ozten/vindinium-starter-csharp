using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //what you use as the number of turns
            ServerStuff serverStuff = new ServerStuff(args[0], args[1] != "arena", int.Parse(args[2]), new Uri(args[3]), null);

            //create the random bot, replace this with your own bot
            RandomBot bot = new RandomBot();

            //now kick it all off by running the bot.
            serverStuff.Submit(bot);

            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}

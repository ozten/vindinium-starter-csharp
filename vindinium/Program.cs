using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium
{
    class Program
    {
        static void Main(string[] args)
        {
            //create the server stuff, when not in training mode, it doesnt matter
            //what you use as the number of turns
            ServerStuff serverStuff = new ServerStuff("my secret key", true, 300, null);

            //create the random bot, replace this with your own bot
            RandomBot bot = new RandomBot(serverStuff);

            //now kick it all off by running the bot.
            bot.run();

            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}

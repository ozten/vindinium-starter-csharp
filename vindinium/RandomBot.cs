using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vindinium
{
    class RandomBot
    {
        private ServerStuff serverStuff;

        public RandomBot(ServerStuff serverStuff)
        {
            this.serverStuff = serverStuff;
        }

        //starts everything
        public void run()
        {
            Console.Out.WriteLine("random bot running");

            serverStuff.createGame();

            if (serverStuff.errored == false)
            {
                //opens up a webpage so you can view the game, doing it async so we dont time out
                new Thread(delegate()
                {
                    System.Diagnostics.Process.Start(serverStuff.viewURL);
                }).Start();
            }
            
            Random random = new Random();
            while (serverStuff.finished == false && serverStuff.errored == false)
            {
                switch(random.Next(0, 6))
                {
                    case 0:
                        serverStuff.moveHero(Direction.East);
                        break;
                    case 1:
                        serverStuff.moveHero(Direction.North);
                        break;
                    case 2:
                        serverStuff.moveHero(Direction.South);
                        break;
                    case 3:
                        serverStuff.moveHero(Direction.Stay);
                        break;
                    case 4:
                        serverStuff.moveHero(Direction.West);
                        break;
                }

                Console.Out.WriteLine("completed turn " + serverStuff.currentTurn);
            }

            if (serverStuff.errored)
            {
                Console.Out.WriteLine("error: " + serverStuff.errorText);
            }

            Console.Out.WriteLine("random bot finished");
        }
    }
}

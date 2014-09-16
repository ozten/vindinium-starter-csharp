using System;
using System.Configuration;
using System.Collections.Generic;
using log4net.Config;

namespace Vindinium.Example
{
    static class Client
    {
        static void Main()
        {
            XmlConfigurator.Configure();
            ServerStuff.Start();
            Console.Out.WriteLine("done");
            Console.Read();
        }
    }
}

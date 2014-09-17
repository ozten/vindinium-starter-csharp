namespace Vindinium.Example
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using log4net.Config;

    internal static class Client
    {
        internal static void Main()
        {
            XmlConfigurator.Configure();
            ServerStuff.Start();
            Console.Out.WriteLine("done");
            Console.Read();
        }
    }
}

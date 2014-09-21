using System;
using Vindinium.Common;

namespace Vindinium.Client.Console
{
    internal struct Parameters
    {
        public string ApiKey;
        public Uri ApiUri;
        public EnvironmentType Environment;
        public uint NumberOfGames;
        public uint Turns;
    }
}
using System;
using Vindinium.Common.DataStructures;

namespace Vindinium.Client.Logic
{
    public class GameEventArgs : EventArgs
    {
        public string Json { get; set; }
        public bool IsArena { get; set; }
        public Game Game { get; set; }
    }
}
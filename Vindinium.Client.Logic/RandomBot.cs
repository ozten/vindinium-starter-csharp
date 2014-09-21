using System;
using Vindinium.Common;

namespace Vindinium.Client.Logic
{
    public class RandomBot
    {
        public Direction DetermineNextMove()
        {
            var values = (Direction[]) Enum.GetValues(typeof (Direction));
            return values[new Random().Next(0, values.Length)];
        }
    }
}
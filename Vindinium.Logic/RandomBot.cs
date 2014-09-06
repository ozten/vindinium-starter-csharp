using System;
using Vindinium.Common;

namespace Vindinium.Logic
{
	public class RandomBot
	{
		public Direction DetermineNextMove()
		{
			return (Direction) (new Random().Next(0, 6));
		}
	}
}
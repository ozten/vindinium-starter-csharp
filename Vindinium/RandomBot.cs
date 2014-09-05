using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vindinium
{
	public class RandomBot : IBot
	{
		Random random = new Random();

		public String Name
		{
			get { return "random bot"; }
		}

		public Direction Move(GameState gameState)
		{
			var i = random.Next(0, 6);
			var outp = i == 0 ? Direction.East : 
				i == 1 ? Direction.North :
				i == 2 ? Direction.South : 
				i == 3 ? Direction.Stay :
				Direction.West;
			Console.WriteLine(outp);
			return outp;
		}

	}
}

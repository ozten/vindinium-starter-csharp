using System;

namespace Vindinium
{
	internal class Client
	{
		private static void Main(string[] args)
		{
			Console.Out.WriteLine("Starting...");
			args = new[] { "xxxx", "training", "30" };
			var serverUrl = args.Length == 4 ? args[3] : "http://vindinium.org";

			var apiEndpoints = new ApiEndpoints(serverUrl, args[0]);
			var gameManager = new GameManager(new ApiCaller(), apiEndpoints);
			var bot = new RandomBot();

			if(args[1] == "arena")
			{
				gameManager.StartArena();
			}
			else
			{
				gameManager.StartTraining(uint.Parse(args[2]));
			}

			Console.Out.WriteLine("Watch the game: {0}", gameManager.ViewUrl);

			
			while (gameManager.Finished == false && gameManager.GameHasError == false)
			{
				var nextMove = bot.DetermineNextMove();
				gameManager.MoveHero(nextMove);
			}
			if(gameManager.GameHasError)
			{
				Console.Out.WriteLine("Error: {0}", gameManager.GameErrorMessage);
			}
			Console.Out.WriteLine("done");
			Console.Read();
		}
	}
}
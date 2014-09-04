using System;

namespace Vindinium
{
	internal class Client
	{
		private static void Main(string[] args)
		{
			Console.Out.WriteLine("Starting...");
			args = new[] { "my secret key", "training", "30" };
			var serverUrl = args.Length == 4 ? args[3] : "http://vindinium.org";
			var key = args[0];
			var mode = args[1];
			var turns = args[2];
			var gameManager = new GameManager(key, mode != "arena", uint.Parse(turns), serverUrl, null);
			var bot = new RandomBot();

			gameManager.StartNewGame();
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
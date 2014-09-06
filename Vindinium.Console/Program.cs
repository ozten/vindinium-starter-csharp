using System;
using System.IO;
using Vindinium.Logic;

namespace Vindinium.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines(@"\vindinium.txt");
			var apiKey = lines[0];
			var environment = lines[1];
			var turns = uint.Parse(lines[3]);
			var apiUrl = new Uri(lines[4], UriKind.Absolute);

			System.Console.Out.WriteLine("API Url: {0}", apiUrl);
			System.Console.Out.WriteLine("API Key: {0}", apiKey);
			System.Console.Out.WriteLine("Environment: {0}", environment);
			System.Console.Out.WriteLine("Turns: {0}", turns);
			System.Console.Out.WriteLine("Starting...");

			var apiEndpoints = new ApiEndpointBuilder(apiUrl, apiKey);
			var gameManager = new GameManager(new ApiCaller(), apiEndpoints, new JsonDeserializer());
			var bot = new RandomBot();

			if (environment == "arena")
			{
				gameManager.StartArena();
			}
			else
			{
				gameManager.StartTraining(turns);
			}

			System.Console.Out.WriteLine("Watch the game: {0}", gameManager.ViewUrl);


			while (gameManager.Finished == false && gameManager.GameHasError == false)
			{
				var nextMove = bot.DetermineNextMove();
				gameManager.MoveHero(nextMove);
			}
			if (gameManager.GameHasError)
			{
				System.Console.Out.WriteLine("Error: {0}", gameManager.GameErrorMessage);
			}
			System.Console.Out.WriteLine("done");
			System.Console.Read();
		}
	}
}

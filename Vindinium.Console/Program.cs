using System;
using System.IO;
using Vindinium.Common;
using Vindinium.Logic;

namespace Vindinium.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				Parameters parameters = GetParameters(args);

				System.Console.Out.WriteLine();
				System.Console.Out.WriteLine("Press [Enter] key when ready");
				System.Console.ReadLine();

				System.Console.Out.WriteLine("Starting...");

				AcceptChallenge(parameters);
			}
			catch (Exception ex)
			{
				System.Console.ForegroundColor = ConsoleColor.Red;
				System.Console.Out.WriteLine(ex.Message);
				System.Console.ResetColor();
			}
			finally
			{
				System.Console.Out.WriteLine("done");
				System.Console.ReadLine();
			}
		}

		private static void AcceptChallenge(Parameters parameters)
		{
			var apiEndpoints = new ApiEndpointBuilder(parameters.ApiUri, parameters.ApiKey);
			var gameManager = new GameManager(new ApiCaller(), apiEndpoints, new JsonDeserializer());
			var bot = new RandomBot();

			StartGameEnvironment(gameManager, parameters);

			System.Console.Out.WriteLine("Watch the game: {0}", gameManager.ViewUrl);

			PlayGame(bot, gameManager);

			if (gameManager.GameHasError)
			{
				System.Console.Out.WriteLine("Error: {0}", gameManager.GameErrorMessage);
			}
		}

		private static void PlayGame(RandomBot bot, GameManager gameManager)
		{
			while (gameManager.Finished == false && gameManager.GameHasError == false)
			{
				Direction nextMove = bot.DetermineNextMove();
				gameManager.MoveHero(nextMove);
			}
		}

		private static void StartGameEnvironment(GameManager gameManager, Parameters parameters)
		{
			if (parameters.Environment == "arena")
			{
				gameManager.StartArena();
			}
			else
			{
				gameManager.StartTraining(parameters.Turns);
			}
		}

		private static Parameters GetParameters(string[] args)
		{
			string[] lines = args ?? File.ReadAllLines(@"\vindinium.txt");

			var parameters = new Parameters
			                 	{
			                 		ApiKey = lines[0],
			                 		Environment = lines[1],
			                 		Turns = uint.Parse(lines[2]),
			                 		ApiUri = new Uri(lines[3], UriKind.Absolute)
			                 	};

			DisplayParameter("API Uri", parameters.ApiUri);
			DisplayParameter("API Key", parameters.ApiKey);
			DisplayParameter("Environment", parameters.Environment);
			DisplayParameter("Turns", parameters.Turns);

			return parameters;
		}

		private static void DisplayParameter(string name, object value)
		{
			System.Console.ForegroundColor = ConsoleColor.Green;
			System.Console.Out.Write("{0,18}: ", name);
			System.Console.ResetColor();
			System.Console.Out.WriteLine(value);
		}
	}
}
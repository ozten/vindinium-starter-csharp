using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vindinium.Client.Logic;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Client.Console
{
	internal class Program
	{
		private static readonly Logger Logger = new Logger();

		private static void Main(string[] args)
		{
			try
			{
				Parameters parameters = GetParameters(args);

				System.Console.Out.WriteLine();
				System.Console.Out.WriteLine("Press the [Enter] key when you are ready.");
				System.Console.ReadLine();
				for (int i = 0; i < parameters.NumberOfGames; i++)
				{
					Logger.Info("Game {0} of {1}", i + 1, parameters.NumberOfGames);
					AcceptChallenge(parameters);
				}
			}
			catch (Exception ex)
			{
				Logger.Fatal("Uncaught exception", ex);
			}
			finally
			{
				System.Console.Out.WriteLine("The games have completed. Press the [Enter] key to exit.");
				System.Console.ReadLine();
			}
		}

		private static void AcceptChallenge(Parameters parameters)
		{
			Logger.Debug("Challenge Accepted");

			var apiEndpoints = new ApiEndpointBuilder(parameters.ApiUri, parameters.ApiKey);
			var gameManager = new GameManager(new ApiCaller(Logger), apiEndpoints, new JsonDeserializer());
			gameManager.GotResponse += SaveResponseForTesting;
			var bot = new RandomBot();

			StartGameEnvironment(gameManager, parameters);

			Logger.Debug("View URL: {0}", gameManager.ViewUrl);

			PlayGame(bot, gameManager);
			if (gameManager.GameHasError)
			{
				Logger.Error(gameManager.GameErrorMessage);
			}
		}

		private static void SaveResponseForTesting(object sender, GameEventArgs gameEventArgs)
		{
			string path = @"\vindinium.logs\";
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			path = Path.Combine(path, gameEventArgs.IsArena ? "arena" : "training");
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			path = Path.Combine(path, string.Format("{0:000}", gameEventArgs.Game.Board.Size));
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			path = Path.Combine(path, gameEventArgs.Game.Id);
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			path = Path.Combine(path, string.Format("{0:0000}.json", gameEventArgs.Game.Turn));
			File.WriteAllText(path, gameEventArgs.Json);
		}

		private static void PlayGame(RandomBot bot, GameManager gameManager)
		{
			while (gameManager.Finished == false && gameManager.GameHasError == false)
			{
				Logger.Info("Life: {0}", gameManager.MyHero.Life);

				Direction nextMove = bot.DetermineNextMove();
				gameManager.MoveHero(nextMove);
			}
			if (!gameManager.GameHasError)
			{
				LogEndGameResults(gameManager);
			}
		}

		private static void LogEndGameResults(GameManager gameManager)
		{
			List<Hero> heroes = gameManager.Heroes;
			int maxGold = heroes.Max(h => h.Gold);
			Hero[] topHeroes = heroes.Where(h => h.Gold == maxGold).ToArray();
			int count = topHeroes.Count();
			if (count == 1)
			{
				Hero winner = topHeroes.First();
				if (winner == gameManager.MyHero)
				{
					Logger.Info("Game Won");
				}
				else
				{
					Logger.Info("Game Lost. Winner is {0}", winner.Name);
				}
			}
			else
			{
				Logger.Info("Game was a draw");
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
			string[] lines = args.Length == 0 ? File.ReadAllLines(@"\vindinium.txt") : args;

			var parameters = new Parameters
			                 	{
			                 		ApiKey = lines[0],
			                 		Environment = lines[1],
			                 		Turns = uint.Parse(lines[2]),
			                 		ApiUri = new Uri(lines[3], UriKind.Absolute),
			                 		NumberOfGames = uint.Parse(lines[4])
			                 	};

			Logger.Debug("API Uri: {0}", parameters.ApiUri);
			Logger.Debug("API Key: {0}", parameters.ApiKey);
			Logger.Debug("Environment: {0}", parameters.Environment);
			Logger.Debug("Turns: {0}", parameters.Turns);
			Logger.Debug("Number of Games: {0}", parameters.NumberOfGames);

			return parameters;
		}
	}
}
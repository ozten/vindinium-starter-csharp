using System;
using System.IO;
using Vindinium.Common;
using Vindinium.Logic;

namespace Vindinium.Console
{
	internal class Program
	{
		private static readonly Logger _logger = new Logger();

		private static void Main(string[] args)
		{
			try
			{
				Parameters parameters = GetParameters(args);

				System.Console.Out.WriteLine();
				System.Console.Out.WriteLine("Press [Enter] key when ready");
				System.Console.ReadLine();
				AcceptChallenge(parameters);
			}
			catch (Exception ex)
			{
				_logger.Fatal("Uncaught exception", ex);
			}
			finally
			{
				System.Console.Out.WriteLine("done");
				System.Console.ReadLine();
			}
		}

		private static void AcceptChallenge(Parameters parameters)
		{
			_logger.Debug("Challenge Accepted");

			var apiEndpoints = new ApiEndpointBuilder(parameters.ApiUri, parameters.ApiKey);
			var gameManager = new GameManager(new ApiCaller(_logger), apiEndpoints, new JsonDeserializer());
			var bot = new RandomBot();

			StartGameEnvironment(gameManager, parameters);

			_logger.Debug("View URL: {0}", gameManager.ViewUrl);

			PlayGame(bot, gameManager);

			if (gameManager.GameHasError)
			{
				_logger.Error(gameManager.GameErrorMessage);
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
			string[] lines = args.Length == 0 ? File.ReadAllLines(@"\vindinium.txt") : args;

			var parameters = new Parameters
			                 	{
			                 		ApiKey = lines[0],
			                 		Environment = lines[1],
			                 		Turns = uint.Parse(lines[2]),
			                 		ApiUri = new Uri(lines[3], UriKind.Absolute)
			                 	};

			_logger.Debug("API Uri: {0}", parameters.ApiUri);
			_logger.Debug("API Key: {0}", parameters.ApiKey);
			_logger.Debug("Environment: {0}", parameters.Environment);
			_logger.Debug("Turns: {0}", parameters.Turns);

			return parameters;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vindinium.Client.Logic;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Services;

namespace Vindinium.Client.Console
{
    internal static class Program
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
            var apiCaller = new ApiCaller();
            var apiEndpoints = new ApiEndpointBuilder(parameters.ApiUri, parameters.ApiKey);
            IGameServerProxy server = new GameServerProxy(apiCaller, apiEndpoints);
            var bot = new RandomBot();

            StartGameEnvironment(server, parameters);

            Logger.Debug("View URL: {0}", server.GameResponse.ViewUrl);

            PlayGame(bot, server, server.GameResponse.Game.Id, server.GameResponse.Token, parameters);
            if (server.ApiResponse.HasError)
            {
                Logger.Error(server.ApiResponse.ErrorMessage);
            }
            else
            {
                LogEndGameResults(server.GameResponse);
            }
        }

        private static void SaveResponseForTesting(GameResponse response, bool isArena, Direction direction)
        {
            string path = @"\vindinium.logs\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, isArena ? "arena" : "training");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, string.Format("{0:000}", response.Game.Board.Size));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, response.Game.Id);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, string.Format("{0:0000}.{1}.json", response.Game.Turn, direction));
            File.WriteAllText(path, response.ToJson());
        }

        private static void PlayGame(RandomBot bot, IGameServerProxy server, string gameId, string token,
            Parameters parameters)
        {
            do
            {
                Direction direction = bot.DetermineNextMove();
                server.Play(gameId, token, direction);

                if (server.ApiResponse.HasError)
                {
                    Logger.Error(server.ApiResponse.ErrorMessage);
                    return;
                }

                SaveResponseForTesting(server.GameResponse, parameters.Environment == EnvironmentType.Arena, direction);
            } while (server.ApiResponse.HasError == false && server.GameResponse.Game.Finished == false &&
                     server.GameResponse.Self.Crashed == false);
        }

        private static void LogEndGameResults(GameResponse gameResponse)
        {
            List<Hero> heroes = gameResponse.Game.Players;
            int maxGold = heroes.Max(h => h.Gold);
            Hero[] topHeroes = heroes.Where(h => h.Gold == maxGold).ToArray();
            int count = topHeroes.Count();
            if (count == 1)
            {
                Hero winner = topHeroes.First();
                if (winner == gameResponse.Self)
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

        private static void StartGameEnvironment(IGameServerProxy server, Parameters parameters)
        {
            if (parameters.Environment == EnvironmentType.Arena)
            {
                server.StartArena();
            }
            else
            {
                server.StartTraining(parameters.Turns);
            }
        }

        private static Parameters GetParameters(string[] args)
        {
            string[] lines = args.Length == 0 ? File.ReadAllLines(@"\vindinium.txt") : args;

            var parameters = new Parameters
            {
                ApiKey = lines[0],
                Environment = (EnvironmentType) Enum.Parse(typeof (EnvironmentType), lines[1], true),
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
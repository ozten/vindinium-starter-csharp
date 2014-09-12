using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vindinium.Client.Logic;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

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
            var apiCaller = new ApiCaller();
            var apiEndpoints = new ApiEndpointBuilder(parameters.ApiUri, parameters.ApiKey);
            var server = new GameServerPoxy(apiCaller, apiEndpoints);
            var bot = new RandomBot();

            IApiResponse response = StartGameEnvironment(server, parameters);
            var game = response.Text.JsonToObject<GameResponse>();

            Logger.Debug("View URL: {0}", game.ViewUrl);

            IApiResponse lastResponse = PlayGame(bot, server, game.Game.Id, game.Token, parameters);
            if (lastResponse.HasError)
            {
                Logger.Error(lastResponse.ErrorMessage);
            }
            else
            {
                LogEndGameResults(lastResponse.Text.JsonToObject<GameResponse>());
            }
        }

        private static void SaveResponseForTesting(GameResponse response, bool isArena)
        {
            string path = @"\vindinium.logs\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, isArena ? "arena" : "training");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, string.Format("{0:000}", response.Game.Board.Size));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, response.Game.Id);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, string.Format("{0:0000}.json", response.Game.Turn));
            File.WriteAllText(path, response.ToJson());
        }

        private static IApiResponse PlayGame(RandomBot bot, IGameServerPoxy server, string gameId, string token,
            Parameters parameters)
        {
            IApiResponse response;
            GameResponse gameResponse;
            do
            {
                response = server.Play(gameId, token, bot.DetermineNextMove());

                if (response.HasError)
                {
                    Logger.Error(response.ErrorMessage);
                    return response;
                }

                gameResponse = response.Text.JsonToObject<GameResponse>();
                SaveResponseForTesting(gameResponse, parameters.Environment == EnvironmentType.Arena);
            } while (response.HasError == false && gameResponse.Game.Finished == false &&
                     gameResponse.Self.Crashed == false);
            return response;
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

        private static IApiResponse StartGameEnvironment(GameServerPoxy server, Parameters parameters)
        {
            if (parameters.Environment == EnvironmentType.Arena)
            {
                return server.StartArena(parameters.ApiKey);
            }
            return server.StartTraining(parameters.ApiKey, parameters.Turns);
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
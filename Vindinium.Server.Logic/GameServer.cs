using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Game.Logic
{
    public class GameServer : IGameServerProxy
    {
        private const string Tavern = "[]";
        private const string MinePrefix = "$";
        private const string NeutralMine = "$-";
        private const string OpenPath = "  ";
        private const string Obstruction = "##";
        private const string PlayerPrefix = "@";
        private GameResponse _response = new GameResponse();

        public GameResponse GameResponse
        {
            get { return null; }
        }

        public IApiResponse ApiResponse
        {
            get { return null; }
        }

        public string Play(string gameId, string token, Direction direction)
        {
            var map = new Grid {MapText = _response.Game.Board.MapText};
            lock (map.SynchronizationRoot)
            {
                Hero player = _response.Game.Players.First(p => p.Id == _response.Self.Id);
                Pos playerPos = player.Pos;
                Pos targetPos = playerPos + GetTargetOffset(direction);
                KeepPositionOnMap(targetPos, _response.Game.Board.Size);
                string targetToken = map[targetPos];

                PlayerMoving(playerPos, map, targetToken, targetPos, player);
                PlayerMoved(direction, targetToken, player, map);
                MoveDeadPlayers(map);
                MakePlayerThirsty(player);
                ApplyMapChanges(map);
                ReviveDeadPlayers();
                IncrimentGold(player);
            }

            return _response.ToJson();
        }

        public string StartTraining(uint turns)
        {
            return Start(EnvironmentType.Training);
        }

        public string StartArena()
        {
            return Start(EnvironmentType.Arena);
        }

        public string Start(string mapText)
        {
            string gameId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string token = Guid.NewGuid().ToString("N").Substring(0, 8);

            var grid = new Grid {MapText = mapText};
            _response = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = new Board {MapText = grid.MapText, Size = grid.Size},
                    Finished = false,
                    Id = gameId,
                    MaxTurns = 20,
                    Players = new List<Hero>(),
                    Turn = 0
                },
                PlayUrl = string.Format("http://vindinium.org/api/{0}/{1}/play", gameId, token),
                Self = CreateHero(mapText, grid, 1),
                Token = token,
                ViewUrl = string.Format("http://vindinium.org/{0}", gameId)
            };
            for (int i = 0; i < 10; i++)
            {
                if (grid.PositionOf(string.Format("{0}{1}", PlayerPrefix, i)) != null)
                {
                    _response.Game.Players.Add(CreateHero(mapText, grid, i));
                }
            }
            return _response.ToJson();
        }

        public void ChangeMap(string mapText)
        {
            ApplyMapChanges(new Grid {MapText = mapText});
        }

        private void ReviveDeadPlayers()
        {
            _response.Game.Players.Where(p => p.Life <= 0).ToList().ForEach(p => p.Life = 100);
        }

        private void IncrimentGold(Hero player)
        {
            player.Gold += player.MineCount;
            _response.Self = _response.Game.Players.First(p => p.Id == 1);
        }

        private void MakePlayerThirsty(Hero player)
        {
            if (player.Life > 1)
            {
                player.Life--;
            }
        }

        private void MoveDeadPlayers(Grid map)
        {
            Hero[] deadPlayers = _response.Game.Players.Where(p => p.Life <= 0 && p.Pos != p.SpawnPos).ToArray();
            do
            {
                foreach (Hero deadPlayer in deadPlayers)
                {
                    string playerMine = string.Format("{0}{1}", MinePrefix, deadPlayer.Id);
                    ReplaceMapToken(map, playerMine, NeutralMine);
                    _response.Game.Players.Where(p => p.Pos == deadPlayer.SpawnPos).ToList().ForEach(p => p.Life = 0);
                    map[deadPlayer.Pos] = OpenPath;
                    deadPlayer.Pos = deadPlayer.SpawnPos;
                }
                deadPlayers = _response.Game.Players.Where(p => p.Life <= 0 && p.Pos != p.SpawnPos).ToArray();
            } while (deadPlayers.Any());

            _response.Game.Players.ForEach(p => { map[p.Pos] = string.Format("{0}{1}", PlayerPrefix, p.Id); });
        }

        private static void ReplaceMapToken(Grid map, string oldToken, string newToken)
        {
            map.ForEach(p =>
            {
                if (map[p] == oldToken)
                {
                    map[p] = newToken;
                }
            });
        }

        private void ApplyMapChanges(Grid map)
        {
            _response.Game.Players.ForEach(p => RefreshHero(p.Id, map));
            _response.Game.Board.MapText = map.MapText;
        }

        private void RefreshHero(int playerId, Grid grid)
        {
            Hero player = _response.Game.Players.First(p => p.Id == playerId);
            string heroToken = string.Format("{0}{1}", PlayerPrefix, playerId);
            string mineToken = string.Format("{0}{1}", MinePrefix, playerId);
            player.Pos = grid.PositionOf(heroToken);
            player.MineCount = Regex.Matches(grid.MapText, Regex.Escape(mineToken)).Count;
        }

        private void Start()
        {
            Start(GenerateMap());
        }

        private string GenerateMap()
        {
            var grid = new Grid();
            int seed = (int) DateTime.Now.Ticks%int.MaxValue;
            var random = new Random(seed);
            int quarter = random.Next(5, 14);
            int size = quarter*2;
            grid.MapText = "".PadLeft(size*size*2, ' ');
            AddToken(grid, Tavern, random);
            AddToken(grid, string.Format("{0}-", PlayerPrefix), random);
            for (int i = 0; i < 4; i++)
            {
                AddToken(grid, NeutralMine, random);
            }
            for (int i = 0; i < 8; i++)
            {
                AddToken(grid, Obstruction, random);
            }

            grid.MakeSymmetric();
            return grid.MapText;
        }

        private void AddToken(Grid grid, string token, Random random)
        {
            int max = grid.Size/2;

            var pos = new Pos {X = random.Next(1, max), Y = random.Next(1, max)};
            while (grid[pos] != OpenPath)
            {
                pos.X = random.Next(1, max);
                pos.Y = random.Next(1, max);
            }
            grid[pos] = token;
        }

        private static Hero CreateHero(string mapText, Grid grid, int playerId)
        {
            string heroToken = string.Format("{0}{1}", PlayerPrefix, playerId);
            string mineToken = string.Format("{0}{1}", MinePrefix, playerId);
            Pos position = grid.PositionOf(heroToken);
            int mineCount = Regex.Matches(mapText, Regex.Escape(mineToken)).Count;

            return new Hero
            {
                Id = playerId,
                Name = playerId == 1 ? "GrimTrick" : "random",
                UserId = playerId == 1 ? "8aq2nq2b" : null,
                Elo = 1213,
                Pos = position,
                Life = 100,
                Gold = 0,
                MineCount = mineCount,
                SpawnPos = position,
                Crashed = false
            };
        }

        private void PlayerMoved(Direction direction, string targetToken, Hero player, Grid map)
        {
            string selfTokene = string.Format("{0}{1}", PlayerPrefix, player.Id);
            if (targetToken.StartsWith(PlayerPrefix) && direction != Direction.Stay && targetToken != selfTokene)
            {
                int enemyId = int.Parse(targetToken.Substring(1));
                Hero enemy = _response.Game.Players.First(p => p.Id == enemyId);
                enemy.Life -= 20;
                string selfMine = string.Format("{0}{1}", MinePrefix, player.Id);
                string enemyMine = string.Format("{0}{1}", MinePrefix, enemy.Id);
                if (enemy.Life <= 0)
                {
                    map.ForEach(p => { if (map[p] == enemyMine) map[p] = selfMine; });
                }
            }
        }

        private void PlayerMoving(Pos playerPos, Grid map, string targetToken, Pos targetPos, Hero player)
        {
            if (targetToken == OpenPath)
            {
                StepOntoPath(targetToken, targetPos, playerPos, map);
            }
            else if (targetToken == Tavern)
            {
                StepIntoTavern();
            }
            else if (targetToken.StartsWith(MinePrefix))
            {
                StepIntoMine(map, targetPos, targetToken, player);
            }
        }

        private static void KeepPositionOnMap(Pos position, int mapSize)
        {
            if (position.X < 1) position.X = 1;
            if (position.Y < 1) position.Y = 1;
            if (position.X > mapSize) position.X = mapSize;
            if (position.Y > mapSize) position.Y = mapSize;
        }

        private void StepOntoPath(string targetToken, Pos targetPos, Pos playerPos, Grid map)
        {
            string playerToken = map[playerPos];
            map[targetPos] = playerToken;
            map[playerPos] = targetToken;
            _response.Game.Players.Where(p => string.Format("{0}{1}", PlayerPrefix, p.Id) == playerToken)
                .AsParallel()
                .ForAll(
                    p => p.Pos = targetPos);
        }

        private void StepIntoMine(Grid map, Pos targetPos, string targetToken, Hero player)
        {
            string playerMine = string.Format("{0}{1}", MinePrefix, player.Id);
            if (targetToken != playerMine)
            {
                player.Life -= 20;
                if (player.Life > 0)
                {
                    map[targetPos] = playerMine;
                }
            }
        }

        private void StepIntoTavern()
        {
            if (_response.Self.Gold >= 2)
            {
                _response.Self.Life += 50;
                _response.Game.Players.First(p => p.Id == _response.Self.Id).Gold
                    -= 2;
                if (_response.Self.Life > 100)
                {
                    _response.Self.Life = 100;
                }
            }
        }

        private Pos GetTargetOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Pos {Y = -1};
                case Direction.South:
                    return new Pos {Y = 1};
                case Direction.West:
                    return new Pos {X = -1};
                case Direction.East:
                    return new Pos {X = 1};
                default:
                    return new Pos();
            }
        }


        private string Start(EnvironmentType environmentType)
        {
            Start();
            if (environmentType == EnvironmentType.Training)
            {
                _response.Game.Players.Where(p => p.Id != _response.Self.Id).ToList().ForEach(p => p.Elo = null);
            }
            return _response.ToJson();
        }
    }
}
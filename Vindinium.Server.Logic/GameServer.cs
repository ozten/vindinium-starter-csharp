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
        private const string OpenPath = "  ";
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
                RefreshData(map);
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
            var grid = new Grid {MapText = mapText};
            _response = new GameResponse
            {
                Game = new Common.DataStructures.Game
                {
                    Board = new Board {MapText = grid.MapText, Size = grid.Size},
                    Finished = false,
                    Id = "the-game-id",
                    MaxTurns = 20,
                    Players = new List<Hero>
                    {
                        CreateHero(mapText, grid, 1),
                        CreateHero(mapText, grid, 2),
                        CreateHero(mapText, grid, 3),
                        CreateHero(mapText, grid, 4)
                    },
                    Turn = 0
                },
                PlayUrl = "http://vindinium.org/api/the-game-id/the-token/play",
                Self = CreateHero(mapText, grid, 1),
                Token = "the-token",
                ViewUrl = "http://vindinium.org/the-game-id"
            };
            return _response.ToJson();
        }

        private void RefreshData(Grid map)
        {
            _response.Game.Players.ForEach(p => RefreshHero(p.Id, map));
            _response.Game.Board.MapText = map.MapText;

            Hero self = _response.Game.Players.First(p => p.Id == 1);
            if (self.Life > 1)
            {
                self.Life--;
            }
            _response.Self = self;
        }

        private void RefreshHero(int playerId, Grid grid)
        {
            Hero player = _response.Game.Players.First(p => p.Id == playerId);
            string heroToken = string.Format("{0}{1}", PlayerPrefix, playerId);
            string mineToken = string.Format("{0}{1}", MinePrefix, playerId);
            if (player.Life <= 0)
            {
                grid[player.Pos] = OpenPath;
                grid[player.SpawnPos] = heroToken;
                player.Life = 100;
                player.Pos = player.SpawnPos;
            }
            else
            {
                player.Pos = grid.PositionOf(heroToken);
            }

            int mineCount = 0;
            grid.ForEach(p => { if (grid[p] == mineToken) mineCount++; });

            player.MineCount = mineCount;
            player.Gold += mineCount;
        }

        private void Start()
        {
            Start(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                "        [][]        ",
                "##                ##",
                "$-  @1$-####$-@4  $-",
                "##  ##        ##  ##",
                "                    ",
                "                    ",
                "##  ##        ##  ##",
                "$-  @2$-####$-@3  $-",
                "##                ##",
                "        [][]        "));
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
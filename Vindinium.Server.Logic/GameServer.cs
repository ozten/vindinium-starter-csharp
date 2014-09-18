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
        private const int FullLife = 100;
        private const int HealingAmount = 50;
        private const int HealingCost = 2;
        private const int AttackDamage = 20;
        private const int MinimumThirst = 1;
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
                 _response.Self = _response.Game.Players.First(p => p.Id == 1);
                return _response.ToJson();
            }
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
                Self = CreateHero(grid, 1),
                Token = token,
                ViewUrl = string.Format("http://vindinium.org/{0}", gameId)
            };
            for (int i = 0; i < 10; i++)
            {
                if (grid.PositionOf(TokenHelper.Player(i)) != null)
                {
                    _response.Game.Players.Add(CreateHero(grid, i));
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
            _response.Game.Players.Where(p => p.Life <= 0).ToList().ForEach(p => p.Life = FullLife);
        }

        private void IncrimentGold(Hero player)
        {
            player.Gold += player.MineCount;
        }

        private void MakePlayerThirsty(Hero player)
        {
            if (player.Life > MinimumThirst)
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
                    ReplaceMapToken(map, deadPlayer.MineToken(), TokenHelper.NeutralMine);
                    _response.Game.Players.Where(p => p.Pos == deadPlayer.SpawnPos).ToList().ForEach(p => p.Life = 0);
                    map[deadPlayer.Pos] = TokenHelper.OpenPath;
                    deadPlayer.Pos = deadPlayer.SpawnPos;
                }
                deadPlayers = _response.Game.Players.Where(p => p.Life <= 0 && p.Pos != p.SpawnPos).ToArray();
            } while (deadPlayers.Any());

            _response.Game.Players.ForEach(p => map[p.Pos] = p.PlayerToken());
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
            _response.Game.Players.ForEach(p => UpdateHeroFromMap(p, map));
            _response.Game.Board.MapText = map.MapText;
        }

        private void UpdateHeroFromMap(Hero player, Grid grid)
        {
            player.Pos = grid.PositionOf(player.PlayerToken());
            player.MineCount = Regex.Matches(grid.MapText, Regex.Escape(player.MineToken())).Count;
        }

        private void Start()
        {
            Start(MapMaker.GenerateMap());
        }


        private Hero CreateHero(Grid grid, int playerId)
        {
            var hero = new Hero
            {
                Id = playerId,
                Name = playerId == 1 ? "GrimTrick" : "random",
                UserId = playerId == 1 ? "8aq2nq2b" : null,
                Elo = 1213,
                Life = FullLife,
                Gold = 0,
                Crashed = false
            };
            UpdateHeroFromMap(hero, grid);
            hero.SpawnPos = hero.Pos;
            return hero;
        }

        private void PlayerMoved(Direction direction, string targetToken, Hero player, Grid map)
        {
            if (SteppedIntoEnemy(player, direction, targetToken))
            {
                int enemyId = int.Parse(targetToken.Substring(1));
                Hero enemy = _response.Game.Players.First(p => p.Id == enemyId);
                enemy.Life -= AttackDamage;
                if (enemy.Life <= 0)
                {
                    map.ForEach(p => { if (map[p] == enemy.MineToken()) map[p] = player.MineToken(); });
                }
            }
        }

        private static bool SteppedIntoEnemy(Hero player, Direction direction, string targetToken)
        {
            return TokenHelper.IsPlayer(targetToken) && direction != Direction.Stay &&
                   targetToken != player.PlayerToken();
        }

        private void PlayerMoving(Pos playerPos, Grid map, string targetToken, Pos targetPos, Hero player)
        {
            if (targetToken == TokenHelper.OpenPath)
            {
                StepOntoPath(targetToken, targetPos, playerPos, map);
            }
            else if (targetToken == TokenHelper.Tavern)
            {
                StepIntoTavern();
            }
            else if (TokenHelper.IsMine(targetToken))
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
            _response.Game.Players.Where(p => p.PlayerToken() == playerToken)
                .AsParallel()
                .ForAll(
                    p => p.Pos = targetPos);
        }

        private void StepIntoMine(Grid map, Pos targetPos, string targetToken, Hero player)
        {
            if (targetToken != player.MineToken())
            {
                player.Life -= AttackDamage;
                if (player.Life > 0)
                {
                    map[targetPos] = player.MineToken();
                }
            }
        }

        private void StepIntoTavern()
        {
            if (_response.Self.Gold >= HealingCost)
            {
                _response.Self.Life += HealingAmount;
                _response.Game.Players.First(p => p.Id == _response.Self.Id).Gold
                    -= HealingCost;
                if (_response.Self.Life > FullLife)
                {
                    _response.Self.Life = FullLife;
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
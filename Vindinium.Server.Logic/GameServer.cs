using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vindinium.Common;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
	public class GameServer
	{
		private GameResponse _response = new GameResponse();

		public GameResponse Start()
		{
			return Start(
				"        [][]        ##                ##$-  @1$-####$-@4  $-##  ##        ##  ##                                        ##  ##        ##  ##$-  @2$-####$-@3  $-##                ##        [][]        ");
		}

		public GameResponse Start(string mapText)
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
			return _response;
		}

		private static Hero CreateHero(string mapText, Grid grid, int playerId)
		{
			string heroToken = string.Format("@{0}", playerId);
			string mineToken = string.Format("${0}", playerId);
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

		public GameResponse Play(string token, Direction direction)
		{
			var map = new Grid {MapText = _response.Game.Board.MapText};
			lock (map.SynchronizationRoot)
			{
				if (_response.Self.Life > 1)
				{
					_response.Self.Life--;
				}


				Pos playerPos = _response.Self.Pos;
				Pos targetPos = playerPos + GetTargetOffset(direction);

				if (targetPos.X < 1) targetPos.X = 1;
				if (targetPos.Y < 1) targetPos.Y = 1;
				string targetToken = map[targetPos.X, targetPos.Y];
				if (targetToken == "  ")
				{
					string playerToken = map[playerPos.X, playerPos.Y];
					map[targetPos] = playerToken;
					map[playerPos] = targetToken;
				}
				else if (targetToken.StartsWith("$"))
				{
					if (targetToken != "$1")
					{
						if (_response.Self.Life <= 20)
						{
							_response.Self.Life = 100;
						}
						else
						{
							_response.Self.Life -= 20;
							_response.Self.MineCount++;
							if (targetToken != "$-")
							{
								int playerId = int.Parse(targetToken.Substring(1, 1));
								_response.Game.Players.Where(p => p.Id == playerId).AsParallel().ForAll(p => p.MineCount--);
							}
							map[targetPos] = "$1";
						}
					}
				}
				_response.Game.Board.MapText = map.MapText;
				_response.Game.Players[0].Life = _response.Self.Life;
				_response.Game.Players[0].MineCount = _response.Self.MineCount;
			}

			return _response;
		}

		private Pos GetTargetOffset(Direction direction)
		{
			switch (direction)
			{
				case Direction.North:
					return new Pos {Y = -1};
				default:
					return new Pos();
			}
		}


		public GameResponse Start(EnvironmentType environmentType)
		{
			_response = Start();
			if (environmentType == EnvironmentType.Training)
			{
				_response.Game.Players.Where(p => p.Id != _response.Self.Id).ToList().ForEach(p => p.Elo = null);
			}
			return _response;
		}
	}
}
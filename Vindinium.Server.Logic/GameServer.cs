using System.Collections.Generic;
using System.Linq;
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
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 1,
			            		       		          				Name = "GrimTrick",
			            		       		          				UserId = "8aq2nq2b",
			            		       		          				Elo = 1213,
			            		       		          				Pos = grid.PositionOf("@1"),
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = grid.PositionOf("@1"),
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 2,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = grid.PositionOf("@2"),
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = grid.PositionOf("@2"),
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 3,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = grid.PositionOf("@3"),
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = grid.PositionOf("@3"),
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 4,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = grid.PositionOf("@4"),
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = grid.PositionOf("@4"),
			            		       		          				Crashed = false
			            		       		          			}
			            		       		          	},
			            		       		Turn = 0
			            		       	},
			            		PlayUrl = "http://vindinium.org/api/the-game-id/the-token/play",
			            		Self = new Hero
			            		       	{
			            		       		Id = 1,
			            		       		Name = "GrimTrick",
			            		       		UserId = "8aq2nq2b",
			            		       		Elo = 1213,
			            		       		Pos = grid.PositionOf("@1"),
			            		       		Life = 100,
			            		       		Gold = 0,
			            		       		MineCount = 0,
			            		       		SpawnPos = grid.PositionOf("@1"),
			            		       		Crashed = false
			            		       	},
			            		Token = "the-token",
			            		ViewUrl = "http://vindinium.org/the-game-id"
			            	};
			return _response;
		}

		public GameResponse Play(string token, Direction north)
		{
			var map = new Grid {MapText = _response.Game.Board.MapText};
			lock (map.SynchronizationRoot)
			{
				if (_response.Self.Life > 1)
				{
					_response.Self.Life--;
				}


				Pos playerPos = _response.Self.Pos;
				var northPos = new Pos {Y = -1};
				Pos targetPos = playerPos + northPos;
				if (targetPos.X < 1) targetPos.X = 1;
				if (targetPos.Y < 1) targetPos.Y = 1;
				string targetToken = map[targetPos.X, targetPos.Y];
				if (targetToken == "  ")
				{
					string playerToken = map[playerPos.X, playerPos.Y];
					map[targetPos] = playerToken;
					map[playerPos] = targetToken;
				}
				else if (targetToken == "$-")
				{
					_response.Self.Life -= 20;

					map[targetPos] = "$1";
				}
				_response.Game.Board.MapText = map.MapText;
				_response.Game.Players[0].Life = _response.Self.Life;
			}

			return _response;
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
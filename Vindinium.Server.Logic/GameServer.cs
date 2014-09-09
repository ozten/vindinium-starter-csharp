using System;
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
			var size = (int) Math.Sqrt(mapText.Length/2d);
			_response = new GameResponse
			            	{
			            		Game = new Common.DataStructures.Game
			            		       	{
			            		       		Board = new Board {MapText = mapText, Size = size},
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
			            		       		          				Pos = new Pos {X = 2, Y = 2},
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = new Pos {X = 2, Y = 2},
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 2,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = new Pos {X = 2, Y = 7},
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = new Pos {X = 2, Y = 7},
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 3,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = new Pos {X = 7, Y = 7},
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = new Pos {X = 7, Y = 7},
			            		       		          				Crashed = false
			            		       		          			},
			            		       		          		new Hero
			            		       		          			{
			            		       		          				Id = 4,
			            		       		          				Name = "random",
			            		       		          				UserId = null,
			            		       		          				Elo = 1200,
			            		       		          				Pos = new Pos {X = 7, Y = 2},
			            		       		          				Life = 100,
			            		       		          				Gold = 0,
			            		       		          				MineCount = 0,
			            		       		          				SpawnPos = new Pos {X = 7, Y = 2},
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
			            		       		Pos = new Pos {X = 2, Y = 2},
			            		       		Life = 100,
			            		       		Gold = 0,
			            		       		MineCount = 0,
			            		       		SpawnPos = new Pos {X = 2, Y = 2},
			            		       		Crashed = false
			            		       	},
			            		Token = "the-token",
			            		ViewUrl = "http://vindinium.org/the-game-id"
			            	};
			return _response;
		}

		public GameResponse Play(string token, Direction north)
		{
			_response.Game.Board.MapText = "@1      ";
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
using System.Collections.Generic;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
	public class GameServer
	{
		public GameResponse Start()
		{
			const string gameId = "gameId";
			const string token = "token";
			const string server = "http://localhost/vindinium";
			const int size = 10;
			return new GameResponse
			       	{
			       		Game = new Common.DataStructures.Game
			       		       	{
			       		       		Id = gameId,
			       		       		Players = new List<Hero> {CreateHero(1), CreateHero(2), CreateHero(3), CreateHero(4)},
			       		       		Turn = 1,
			       		       		MaxTurns = 1200,
			       		       		Board = new Board
			       		       		        	{
			       		       		        		Size = size,
			       		       		        		MapText = "$-  @1@2@3@4[]##".PadLeft(size*size*2, '#')
			       		       		        	}
			       		       	}
			       		,
			       		Self = CreateHero(1),
			       		Token = token,
			       		PlayUrl = string.Format("{0}/api/{1}/{2}/play", server, gameId, token),
			       		ViewUrl = string.Format("{0}/{1}", server, gameId)
			       	};
		}

		private static Hero CreateHero(byte id)
		{
			var pos = new Pos {X = id, Y = 0};
			var spawnPos = new Pos {X = id, Y = 0};
			return new Hero
			       	{
			       		Life = 100,
			       		Id = id,
			       		Pos = pos,
			       		SpawnPos = spawnPos,
			       		Elo = 1200,
			       		Name = "test",
			       		UserId = string.Format("User{0}", id)
			       	};
		}
	}
}
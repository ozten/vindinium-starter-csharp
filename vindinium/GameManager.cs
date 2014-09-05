using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Vindinium.Common;
using Vindinium.Common.DataStructure;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium
{
	internal class GameManager
	{
		private readonly IApiCaller _apiCaller;
		private readonly IApiEndpointBuilder _apiEndpointBuilder;

		private string _playUrl;
		public GameManager(IApiCaller apiCaller, IApiEndpointBuilder apiEndpointBuilder)
		{
			_apiCaller = apiCaller;
			_apiEndpointBuilder = apiEndpointBuilder;
		}


		public string ViewUrl { get; private set; }

		public Hero MyHero { get; private set; }
		public List<Hero> Heroes { get; private set; }
		public List<Hero> PreviousHeroes { get; private set; }

		public int CurrentTurn { get; private set; }
		public int MaxTurns { get; private set; }
		public bool Finished { get; private set; }
		public bool GameHasError { get; private set; }
		public string GameErrorMessage { get; private set; }

		public Board Board { get; private set; }
		public Board PreviousBoard { get; private set; }

		public void StartArena()
		{
			GameHasError = false;
			GameErrorMessage = null;
			var response = _apiCaller.Get(_apiEndpointBuilder.StartArena());
			ProcessResponse(response);
		}
		
		public void StartTraining(uint turns = 30)
		{
			GameHasError = false;
			GameErrorMessage = null;
			var response = _apiCaller.Get(_apiEndpointBuilder.StartTraining(turns));
			ProcessResponse(response);
		}

		private GameResponse Deserialize(string json)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using (var stream = new MemoryStream(byteArray))
			{
				var ser = new DataContractJsonSerializer(typeof (GameResponse));
				return ser.ReadObject(stream) as GameResponse;
			}
		}

		public void MoveHero(Direction direction)
		{
			var response = _apiCaller.Get(_apiEndpointBuilder.Play(_playUrl, direction));
			ProcessResponse(response);
		}

		private void ProcessResponse(IApiResponse result)
		{
			if (result.HasError)
			{
				GameHasError = true;
				GameErrorMessage = result.ErrorMessage;
			}
			else
			{
				var gameResponse = Deserialize(result.Text);
				PreviousHeroes = Heroes;
				PreviousBoard = Board;
				_playUrl = gameResponse.PlayUrl;
				ViewUrl = gameResponse.ViewUrl;
				MyHero = gameResponse.Hero;

				Game game = gameResponse.Game;

				Heroes = game.Heroes;
				CurrentTurn = game.Turn;
				MaxTurns = game.MaxTurns;
				Finished = game.Finished;
				Board = game.Board;
			}
		}
	}
}
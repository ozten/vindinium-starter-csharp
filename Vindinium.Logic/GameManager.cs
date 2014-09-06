using System;
using System.Collections.Generic;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Logic
{
	public class GameManager
	{
		private readonly IApiCaller _apiCaller;
		private readonly IApiEndpointBuilder _apiEndpointBuilder;
		private readonly IJsonDeserializer _jsonDeserializer;

		private Uri _playUrl;

		public GameManager(IApiCaller apiCaller, IApiEndpointBuilder apiEndpointBuilder, IJsonDeserializer jsonDeserializer)
		{
			_apiCaller = apiCaller;
			_apiEndpointBuilder = apiEndpointBuilder;
			_jsonDeserializer = jsonDeserializer;
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

		private void Start(IApiRequest apiRequest)
		{
			GameHasError = false;
			GameErrorMessage = null;
			CallApi(apiRequest);
		}

		public void StartArena()
		{
			Start(_apiEndpointBuilder.StartArena());
		}

		public void StartTraining(uint turns = 30)
		{
			Start(_apiEndpointBuilder.StartTraining(turns));
		}


		public void MoveHero(Direction direction)
		{
			CallApi(_apiEndpointBuilder.Play(_playUrl, direction));
		}

		private void CallApi(IApiRequest apiRequest)
		{
			IApiResponse response = _apiCaller.Call(apiRequest);

			if (response.HasError)
			{
				GameHasError = true;
				GameErrorMessage = response.ErrorMessage;
			}
			else
			{
				var gameResponse = _jsonDeserializer.Deserialize<GameResponse>(response.Text);
				PreviousHeroes = Heroes;
				PreviousBoard = Board;
				_playUrl = new Uri(gameResponse.PlayUrl, UriKind.Absolute);
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
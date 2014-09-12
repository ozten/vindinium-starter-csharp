using System;
using System.Collections.Generic;
using Vindinium.Common;
using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class GameManager
    {
        private readonly IApiCaller _apiCaller;
        private readonly IApiEndpointBuilder _apiEndpointBuilder;
        public EventHandler<GameEventArgs> GotResponse;

        private bool _isArena;
        private Uri _playUrl;

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

        private void Start(IApiRequest apiRequest)
        {
            GameHasError = false;
            GameErrorMessage = null;
            CallApi(apiRequest);
        }

        public void StartArena()
        {
            _isArena = true;
            Start(_apiEndpointBuilder.StartArena());
        }

        public void StartTraining(uint turns = 30)
        {
            _isArena = false;
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
                var gameResponse = response.Text.JsonToObject<GameResponse>();
                var args = new GameEventArgs
                {
                    Json = response.Text,
                    Game = gameResponse.Game,
                    IsArena = _isArena
                };
                if (GotResponse != null) GotResponse(this, args);

                PreviousHeroes = Heroes;
                PreviousBoard = Board;
                _playUrl = new Uri(gameResponse.PlayUrl, UriKind.Absolute);
                ViewUrl = gameResponse.ViewUrl;
                MyHero = gameResponse.Self;

                Game game = gameResponse.Game;

                Heroes = game.Players;
                CurrentTurn = game.Turn;
                MaxTurns = game.MaxTurns;
                Finished = game.Finished;
                Board = game.Board;
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Vindinium.Contracts;

namespace Vindinium
{
	internal class GameManager
	{
		private readonly string _map;
		private readonly string _secretKey;
		private readonly string _serverUrl;
		private readonly bool _trainingMode;
		private readonly uint _turns;

		private string _playUrl;

		public GameManager(string secretKey, bool trainingMode, uint turns, string serverUrl, string map)
		{
			_secretKey = secretKey;
			_trainingMode = trainingMode;
			_serverUrl = serverUrl;

			if (!trainingMode) return;
			_turns = turns;
			_map = map;
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

		public void StartNewGame()
		{
			GameHasError = false;
			GameErrorMessage = null;
			string uri = string.Format("{0}/api/{1}", _serverUrl, _trainingMode ? "training" : "arena");
			var sb = new StringBuilder();
			sb.AppendFormat("key={0}", _secretKey);
			if (_trainingMode) sb.AppendFormat("&turns={0}", _turns);
			if (!string.IsNullOrWhiteSpace(_map)) sb.AppendFormat("&map={0}", _map);
			WebIoResponse response = WebIo.Get(uri, sb.ToString());
			ProcessResponse(response);
		}

		private void Deserialize(string json)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using (var stream = new MemoryStream(byteArray))
			{
				var ser = new DataContractJsonSerializer(typeof (GameResponse));
				var gameResponse = (GameResponse) ser.ReadObject(stream);
				PreviousHeroes = Heroes;
				PreviousBoard = Board;
				_playUrl = gameResponse.PlayUrl;
				ViewUrl = gameResponse.ViewUrl;
				MyHero = gameResponse.Hero;
				Heroes = gameResponse.Game.Heroes;
				CurrentTurn = gameResponse.Game.Turn;
				MaxTurns = gameResponse.Game.MaxTurns;
				Finished = gameResponse.Game.Finished;
				Board = gameResponse.Game.Board;
			}
		}

		public void MoveHero(Direction direction)
		{
			string parameters = string.Format("key={0}&dir={1}", _secretKey, direction);
			WebIoResponse response = WebIo.Get(_playUrl, parameters);
			ProcessResponse(response);
		}

		private void ProcessResponse(WebIoResponse result)
		{
			if (result.HasError)
			{
				GameHasError = true;
				GameErrorMessage = result.ErrorMessage;
			}
			else
			{
				Deserialize(result.Text);
			}
		}
	}
}
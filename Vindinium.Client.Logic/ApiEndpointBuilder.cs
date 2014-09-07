using System;
using Vindinium.Common;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
	public class ApiEndpointBuilder : IApiEndpointBuilder
	{
		private readonly string _apiKey;
		private readonly Uri _startArenaUri;
		private readonly Uri _startTrainingUri;

		public ApiEndpointBuilder(Uri apiUri, string apiKey)
		{
			_apiKey = apiKey;
			_startArenaUri = new Uri(apiUri, "/api/arena");
			_startTrainingUri = new Uri(apiUri, "/api/training");
		}

		#region IApiEndpointBuilder Members

		public IApiRequest StartArena()
		{
			return new ApiRequest
			       	{
			       		Uri = _startArenaUri,
			       		Parameters = string.Format("key={0}", _apiKey)
			       	};
		}

		public IApiRequest StartTraining(uint turns)
		{
			return new ApiRequest
			       	{
			       		Uri = _startTrainingUri,
			       		Parameters = string.Format("key={0}&turns={1}", _apiKey, turns)
			       	};
		}

		public IApiRequest Play(Uri playUrl, Direction direction)
		{
			return new ApiRequest
			       	{
			       		Uri = playUrl,
			       		Parameters = string.Format("key={0}&dir={1}", _apiKey, direction)
			       	};
		}

		#endregion
	}
}
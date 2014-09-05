using Vindinium.Common;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium
{
	public class ApiEndpointBuilder : IApiEndpointBuilder
	{
		private readonly string _url;
		private readonly string _key;

		public ApiEndpointBuilder(string url, string key)
		{
			_key = key;
			_url = url;
		}

		#region IApiEndpointBuilder Members


		public IApiRequest StartArena()
		{
			return new ApiRequest
			       	{
			       		Url = string.Format("{0}/api/{1}", _url, "arena"),
			       		Parameters = string.Format("key={0}", _key)
			       	};
		}

		public IApiRequest StartTraining(uint turns)
		{
			return new ApiRequest
			       	{
			       		Url = string.Format("{0}/api/{1}", _url, "training"),
			       		Parameters = string.Format("key={0}&turns={1}", _key, turns)
			       	};
		}

		public IApiRequest Play(string playUrl, Direction direction)
		{
			return new ApiRequest
			       	{
			       		Url = playUrl,
			       		Parameters = string.Format("key={0}&dir={1}", _key, direction)
			       	};
		}

		#endregion
	}
}
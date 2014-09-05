using Vindinium.Common.Entities;

namespace Vindinium
{
	class ApiRequest : IApiRequest
	{
		public string Url { get; set; }
		public string Parameters { get; set; }
	}
}
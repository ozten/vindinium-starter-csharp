using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium
{
	class ApiRequest : IApiRequest
	{
		public string Url { get; set; }
		public string Parameters { get; set; }
	}
}
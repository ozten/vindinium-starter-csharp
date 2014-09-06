using System;
using Vindinium.Common.Entities;

namespace Vindinium.Logic
{
	class ApiRequest : IApiRequest
	{
		public Uri Uri { get; set; }
		public string Parameters { get; set; }
	}
}
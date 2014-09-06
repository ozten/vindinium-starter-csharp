using System;

namespace Vindinium.Common.Entities
{
	public interface IApiRequest
	{
		Uri Uri { get; }
		string Parameters { get; }
	}
}
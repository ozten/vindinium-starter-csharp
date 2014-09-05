using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium
{
	public class ApiResponse : IApiResponse
	{
		public ApiResponse(string text)
		{
			Text = text;
		}

		private ApiResponse()
		{
		}

		public string ErrorMessage { get; private set; }
		public string Text { get; private set; }
		public bool HasError { get; set; }

		public static ApiResponse GetError(string message)
		{
			return new ApiResponse {HasError = true, ErrorMessage = message};
		}

		public static ApiResponse GetErrorWithoutMessage()
		{
			return new ApiResponse {HasError = true};
		}
	}
}
namespace Vindinium
{
	internal class WebIoResponse
	{
		public WebIoResponse(string text)
		{
			Text = text;
		}

		private WebIoResponse()
		{
		}

		public string ErrorMessage { get; private set; }
		public string Text { get; private set; }
		public bool HasError { get; set; }

		public static WebIoResponse GetError(string message)
		{
			return new WebIoResponse {HasError = true, ErrorMessage = message};
		}

		public static WebIoResponse GetErrorWithoutMessage()
		{
			return new WebIoResponse {HasError = true};
		}
	}
}
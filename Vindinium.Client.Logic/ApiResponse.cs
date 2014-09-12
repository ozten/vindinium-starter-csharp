using Vindinium.Common.Entities;

namespace Vindinium.Client.Logic
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

        #region IApiResponse Members

        public string ErrorMessage { get; private set; }
        public string Text { get; private set; }
        public bool HasError { get; private set; }

        #endregion

        public static ApiResponse GetError(string message)
        {
            return new ApiResponse {HasError = true, ErrorMessage = message};
        }
    }
}
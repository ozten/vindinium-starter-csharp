using System;
using System.IO;
using System.Net;
using System.Text;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Logic
{
	public class ApiCaller : IApiCaller
	{
		private readonly ILogger _logger;

		public ApiCaller(ILogger logger)
		{
			_logger = logger;
		}

		#region IApiCaller Members

		public IApiResponse Call(IApiRequest apiRequest)
		{
			try
			{
				HttpWebRequest request = CreateApiClient(apiRequest);
				
				DateTime started = DateTime.Now;
				using (WebResponse response = request.GetResponse())
				{
					LogResponseTime(started);
					return ResponseAsApiResponse(response);
				}
			}
			catch (WebException exception)
			{
				return ExceptionAsApiResponse(exception);
			}
		}

		#endregion

		private IApiResponse ResponseAsApiResponse(WebResponse response)
		{
			_logger.Trace("ResponseAsApiResponse");
			using (Stream stream = response.GetResponseStream())
			{
				if (stream == null) return new ApiResponse(null);

				using (var reader = new StreamReader(stream))
				{
					string serverResponseText = reader.ReadToEnd();
					
					_logger.Trace(serverResponseText);
					return new ApiResponse(serverResponseText);
				}
			}
		}

		private void LogResponseTime(DateTime startedTime)
		{
			TimeSpan diff = DateTime.Now.Subtract(startedTime);
			_logger.Trace("LogResponseTime: {0,5} miliseconds", diff.TotalMilliseconds);
		}

		private HttpWebRequest CreateApiClient(IApiRequest apiRequest)
		{
			_logger.Trace("CreateApiClient {0} {1}", apiRequest.Uri, apiRequest.Parameters);
			var request = (HttpWebRequest)WebRequest.Create(apiRequest.Uri);
			request.KeepAlive = false;
			request.Method = "Post";
			request.ContentType = "application/x-www-form-urlencoded";
			byte[] buffer = Encoding.UTF8.GetBytes(apiRequest.Parameters);

			request.ContentLength = buffer.Length;
			using (Stream reqStream = request.GetRequestStream())
			{
				reqStream.Write(buffer, 0, buffer.Length);
				reqStream.Close();
			}
			return request;
		}

		private IApiResponse ExceptionAsApiResponse(WebException exception)
		{
			_logger.Trace("ExceptionAsApiResponse - Status: {0}", exception.Status);
			using (Stream responseStream = exception.Response.GetResponseStream())
			{
				if (responseStream != null)
				{
					using (var reader = new StreamReader(responseStream))
					{
						return ApiResponse.GetError(reader.ReadToEnd());
					}
				}
			}
			return ApiResponse.GetErrorWithoutMessage();
		}
	}
}
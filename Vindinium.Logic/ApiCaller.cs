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

		private static IApiResponse ResponseAsApiResponse(WebResponse response)
		{
			using (Stream stream = response.GetResponseStream())
			{
				if (stream == null) return new ApiResponse(null);

				using (var reader = new StreamReader(stream))
				{
					string serverResponseText = reader.ReadToEnd();
					return new ApiResponse(serverResponseText);
				}
			}
		}

		private static void LogResponseTime(DateTime startedTime)
		{
			TimeSpan diff = DateTime.Now.Subtract(startedTime);
			if (diff.TotalMilliseconds > 800)
			{
				Console.ForegroundColor = ConsoleColor.Red;
			}
			else if (diff.TotalMilliseconds > 500)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
			}
			Console.WriteLine("Web request took {0,5} milliseconds", diff.TotalMilliseconds);
			Console.ResetColor();
		}

		private static HttpWebRequest CreateApiClient(IApiRequest apiRequest)
		{
			var request = (HttpWebRequest) WebRequest.Create(apiRequest.Uri);
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

		private static IApiResponse ExceptionAsApiResponse(WebException exception)
		{
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
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Vindinium
{
	internal class WebIo
	{
		internal static WebIoResponse Get(string url, string data)
		{
			try
			{
				var request = (HttpWebRequest) WebRequest.Create(url);
				request.KeepAlive = false;
				request.Method = "Post";
				request.ContentType = "application/x-www-form-urlencoded";
				byte[] buffer = Encoding.UTF8.GetBytes(data);

				request.ContentLength = buffer.Length;
				Stream reqStream = request.GetRequestStream();
				reqStream.Write(buffer, 0, buffer.Length);
				reqStream.Close();

				DateTime now = DateTime.Now;
				using (WebResponse response = request.GetResponse())
				{
					TimeSpan diff = DateTime.Now.Subtract(now);
					if (diff.TotalMilliseconds > 500)
					{
						Console.WriteLine("Web request took {0} milliseconds", diff.TotalMilliseconds);
					}

					using (Stream stream = response.GetResponseStream())
					{
						if (stream == null) return new WebIoResponse(null);

						using (var reader = new StreamReader(stream))
						{
							string serverResponseText = reader.ReadToEnd();
							return new WebIoResponse(serverResponseText);
						}
					}
				}
			}
			catch (WebException exception)
			{
				Stream responseStream = exception.Response.GetResponseStream();
				if (responseStream != null)
				{
					using (var reader = new StreamReader(responseStream))
					{
						return WebIoResponse.GetError(reader.ReadToEnd());
					}
				}
				return WebIoResponse.GetErrorWithoutMessage();
			}
			finally
			{
			}
		}
	}
}
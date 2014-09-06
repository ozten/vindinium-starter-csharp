using System;

namespace Vindinium.Common.Services
{
	public interface ILogger
	{
		void Warn(string message, params object[] args);
		void Info(string message, params object[] args);
		void Debug(string message, params object[] args);
		void Trace(string message, params object[] args);
		void Error(string message, params object[] args);
		void Fatal(string message, params object[] args);
		void Fatal(string message, Exception ex);
		void Log(LogLevel logLevel, string message, params object[] args);
	}
}
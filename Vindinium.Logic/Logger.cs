using System;
using NLog;
using Vindinium.Common.Services;
using LogLevel = Vindinium.Common.LogLevel;

namespace Vindinium.Logic
{
	public class Logger : ILogger
	{
		private readonly NLog.Logger _logger;

		public Logger()
		{
			_logger = LogManager.GetCurrentClassLogger();
		}

		#region ILogger Members

		public void Warn(string message, params object[] args)
		{
			_logger.Warn(message, args);
		}

		public void Error(string message, params object[] args)
		{
			_logger.Error(message, args);
		}

		public void Fatal(string message, params object[] args)
		{
			_logger.Fatal(message, args);
		}

		public void Fatal(string message, Exception exception)
		{
			_logger.Fatal(message, exception);
		}

		public void Info(string message, params object[] args)
		{
			_logger.Info(message, args);
		}

		public void Debug(string message, params object[] args)
		{
			_logger.Debug(message, args);
		}

		public void Trace(string message, params object[] args)
		{
			_logger.Trace(message, args);
		}

		public void Log(LogLevel logLevel, string message, params object[] args)
		{
			switch (logLevel)
			{
				case LogLevel.Warn:
					Warn(message, args);
					break;
				case LogLevel.Debug:
					Debug(message, args);
					break;
				case LogLevel.Info:
					Info(message, args);
					break;
				case LogLevel.Error:
					Error(message, args);
					break;
				case LogLevel.Fatal:
					Fatal(message, args);
					break;
				case LogLevel.Trace:
					Trace(message, args);
					break;
				default:
					Info("Logger Level Unknown: {0}", logLevel);
					Info(message, args);
					break;
			}
		}

		#endregion
	}
}
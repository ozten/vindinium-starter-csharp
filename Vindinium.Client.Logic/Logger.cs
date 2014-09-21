using System;
using NLog;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class Logger : ILogger
    {
        private readonly NLog.Logger _logger;

        public Logger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        #region ILogger Members

        public void Trace(string message, params object[] args)
        {
            _logger.Trace(message, args);
        }

        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
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

        #endregion
    }
}
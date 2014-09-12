namespace Vindinium.Common.Services
{
    public interface ILogger
    {
        void Trace(string message, params object[] args);
    }
}
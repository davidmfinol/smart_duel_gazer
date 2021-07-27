using System;
using Code.Wrappers.WrapperLogger;
using Object = UnityEngine.Object;

namespace Code.Core.Logger
{
    public interface IAppLogger
    {
        void Assert(string tag, string message);
        void Log(string tag, string message);
        void Warning(string tag, string message);
        void Exception(string tag, string message, Exception exception, Object context = null);
    }

    public class AppLogger : IAppLogger
    {
        private readonly ILoggerProvider _logger;

        public AppLogger(
            ILoggerProvider logger)
        {
            _logger = logger;
        }

        // Used for debugging (e.g. logging the value of a parameter)
        public void Assert(string tag, string message)
        {
            _logger.Assert(FormatMessage(tag, message));
        }

        // Used for general logs (e.g. at the top of a public function);
        public void Log(string tag, string message)
        {
            _logger.Log(FormatMessage(tag, message));
        }

        // Used for logging expected errors that are handled
        public void Warning(string tag, string message)
        {
            _logger.Warning(FormatMessage(tag, message));
        }

        // Used for logging unexpected exceptions
        public void Exception(string tag, string message, Exception exception, Object context = null)
        {
            _logger.Exception(FormatMessage(tag, message), exception, context);
        }

        private static string FormatMessage(string tag, string message)
        {
            return $"{tag}: {message}";
        }
    }
}
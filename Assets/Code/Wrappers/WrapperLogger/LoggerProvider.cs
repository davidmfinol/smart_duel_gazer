using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Wrappers.WrapperLogger
{
    public interface ILoggerProvider
    {
        void Assert(object message);
        void Log(object message);
        void Warning(object message);
        void Exception(object message, Exception exception, Object context = null);
    }
    
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Logger _logger;

        public LoggerProvider()
        {
            _logger = new Logger(new LogHandler());
        }

        public void Assert(object message)
        {
            _logger.Log(LogType.Assert, message);
        }

        public void Log(object message)
        {
            _logger.Log(LogType.Log, message);
        }

        public void Warning(object message)
        {
            _logger.Log(LogType.Warning, message);
        }

        public void Exception(object message, Exception exception, Object context = null)
        {
            _logger.Log(LogType.Exception, message);
            _logger.LogException(exception, context);
        }
    }

    internal class LogHandler : ILogHandler
    {
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            Debug.unityLogger.LogException(exception, context);
        }
    }
}
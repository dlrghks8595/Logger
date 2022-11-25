using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using Log4Net.Async;
using System;
using System.Text;

namespace Logger
{
    public class Logger
    {
        private string logFileName, logFileBasePath;
        private bool isLogSetup = false;
        private ILog logManager;
        private ConsoleAppender consoleAppender;
        private RollingFileAppender rollingAppender;
        private MemoryAppenderWithEvent memoryAppender;
        private AsyncForwardingAppender asyncForwardingAppender;
        public event EventHandler<LoggingEvent> OnLogging;
        private void LogEventHandler(object sender, LoggingEvent eventArgs) => OnLogging?.Invoke(sender, eventArgs);

        public ILog Log
        {
            get
            {
                if (isLogSetup == false) CreateLogManager();
                return logManager;
            }
            private set => logManager = value;
        }

        public Logger(string logFileName, string filePath)
        {
            this.logFileName = logFileName;
            logFileBasePath = filePath;
        }
        ~Logger()
        {
            if (logManager != null) logManager.Logger.Repository.Shutdown();
        }
        private void CreateLogManager()
        {
            isLogSetup = true;
            ILog log = LogManager.GetLogger(logFileName);
            log4net.Repository.Hierarchy.Logger logger = (log4net.Repository.Hierarchy.Logger)log.Logger;

            logger.Level = Level.All;
            logger.Repository.Configured = true;

            CreateConsoleAppernder();
            CreateFileAppender();
            CreateMemoryAppender();
            CreateAsyncForwarder();

            logger.AddAppender(asyncForwardingAppender);

            Log = log;
            Log.Info(logFileName + " Log manager created");
        }

        private bool CreateConsoleAppernder()
        {
            consoleAppender = new ConsoleAppender();
            PatternLayout layout = new PatternLayout("%date %level - %message%newline");
            consoleAppender.Layout = layout;
            consoleAppender.ActivateOptions();
            return true;
        }

        private bool CreateFileAppender()
        {
            string fileName = logFileBasePath;

            rollingAppender = new RollingFileAppender();
            rollingAppender.Name = logFileName;
            rollingAppender.File = fileName;
            rollingAppender.Encoding = Encoding.UTF8;
            rollingAppender.AppendToFile = true;
            rollingAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            rollingAppender.LockingModel = new RollingFileAppender.MinimalLock();
            rollingAppender.StaticLogFileName = false;
            rollingAppender.MaxSizeRollBackups = 50;
            rollingAppender.MaximumFileSize = "10MB";

            PatternLayout layout = new PatternLayout("%date [%thread] %level - %message%newline");
            rollingAppender.Layout = layout;
            rollingAppender.ActivateOptions();
            return true;
        }

        private bool CreateMemoryAppender()
        {
            memoryAppender = new MemoryAppenderWithEvent();
            memoryAppender.Updated += LogEventHandler;
            return true;
        }

        private bool CreateAsyncForwarder()
        {
            asyncForwardingAppender = new AsyncForwardingAppender();
            asyncForwardingAppender.AddAppender(consoleAppender);
            asyncForwardingAppender.AddAppender(rollingAppender);
            asyncForwardingAppender.AddAppender(memoryAppender);
            asyncForwardingAppender.ActivateOptions();
            return true;
        }
    }
}

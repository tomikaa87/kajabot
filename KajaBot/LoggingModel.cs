using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace KajaBot
{
    internal class LogEntry
    {
        public string Level { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    internal class LoggingAppender : IAppender
    {
        private const int LogHistoryCapacity = 20;

        private readonly Action<LogEntry> _appendAction;
        private readonly Queue<string> _history = new Queue<string>(LogHistoryCapacity);
        private readonly StreamWriter _logStream;

        public LoggingAppender(Action<LogEntry> appendAction)
        {
            _appendAction = appendAction;

            // Prepare log file
            string path = AppDomain.CurrentDomain.BaseDirectory + "log.txt";
            var file = new FileStream(path, FileMode.Append);
            _logStream = new StreamWriter(file);
            _logStream.AutoFlush = true;

            // Write startup marker into the log file
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("---------- Started: ");
            sb.Append(DateTime.Today.ToString("yyyy-MM-dd "));
            sb.Append(DateTime.Now.ToString("HH:mm.ss"));
            sb.AppendLine(" ----------");

            AppendLine(sb.ToString());
        }

        public string[] History
        {
            get { return _history.ToArray(); }
        }

        public void Close()
        {
            _logStream.Close();
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            var entry = new LogEntry
            {
                Level = loggingEvent.Level.ToString(),
                Name = loggingEvent.LoggerName,
                Message = loggingEvent.RenderedMessage,
                Timestamp = loggingEvent.TimeStamp
            };

            _appendAction(entry);

            // Create log message which will be written into the log file
            var sb = new StringBuilder();

            // Append timestamp
            sb.Append("[");
            sb.Append(loggingEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm.ss"));
            sb.Append("] ");

            // Append level
            sb.Append(loggingEvent.Level.ToString().ToUpper());
            sb.Append(" ");

            // Append name
            sb.Append("(");
            sb.Append(loggingEvent.LoggerName);
            sb.Append(") ");

            // Append message
            sb.AppendLine(loggingEvent.RenderedMessage);

            AppendLine(sb.ToString());
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private void AppendLine(string line)
        {
            if (_history.Count == LogHistoryCapacity)
                _history.Dequeue();

            _history.Enqueue(line);

            _logStream.Write(line);
        }
    }

    internal class LoggingModel
    {
        private static volatile LoggingModel _instance;
        private static readonly object _lock = new object();

        private readonly LoggingAppender _appender;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private LoggingModel()
        {
            Entries = new ObservableCollection<LogEntry>();

            _appender = new LoggingAppender(AddEntry);
            BasicConfigurator.Configure(_appender);
        }

        public string[] History
        {
            get { return _appender.History; }
        }

        public ObservableCollection<LogEntry> Entries { get; private set; }

        public static LoggingModel GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new LoggingModel();
                }
            }

            return _instance;
        }

        public void AddEntry(LogEntry entry)
        {
            _dispatcher.Invoke(() => Entries.Add(entry));
        }
    }
}
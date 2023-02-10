using Penguin.Cms.Errors;
using Penguin.Messaging.Core;
using Penguin.Messaging.Logging;
using Penguin.Messaging.Logging.Extensions;
using Penguin.Persistence.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;

namespace Penguin.Cms.Logging.Services
{
    /// <summary>
    /// A class provided with the intent of logging out messages through as many code paths as possible to ensure nothing is missed
    /// </summary>
    public class Logger : IDisposable
    {
        /// <summary>
        /// Constructs a new instance of this class
        /// </summary>
        /// <param name="caller">The object that is doing the logging</param>
        /// <param name="messageBus">A message bus to send log messages over</param>
        /// <param name="logEntryRepository">An IRepository implememntation for persisting logged messages</param>
        /// <param name="errorRepository">An IRepository implementation for persisting errors</param>
        public Logger(object caller, MessageBus messageBus, IRepository<LogEntry> logEntryRepository, IRepository<AuditableError> errorRepository)
        {
            Contract.Requires(logEntryRepository != null);
            Contract.Requires(caller != null);

            SessionStart = DateTime.Now;
            GUID = Guid.NewGuid().ToString();
            Caller = caller.GetType().ToString();
            Entries = new List<LogEntry>();
            MessageBus = messageBus;
            LogEntryRepository = logEntryRepository;
            ErrorRepository = errorRepository;
            FileName = $"Logs\\{Caller}.{SessionStart.ToString("yyyy.MM.dd.HH.mm.ss", CultureInfo.CurrentCulture)}.log";

            if (!Directory.Exists("Logs"))
            {
                _ = Directory.CreateDirectory("Logs");
            }

            WriteContext = logEntryRepository.WriteContext();
        }

        private string FileName { get; set; }

        private IWriteContext WriteContext { get; set; }

        private string Caller { get; set; }

        private IRepository<LogEntry> LogEntryRepository { get; set; }

        private IRepository<AuditableError> ErrorRepository { get; set; }

        private List<LogEntry> Entries { get; set; }

        private string GUID { get; set; }

        private DateTime SessionStart { get; set; }

        private MessageBus MessageBus { get; set; }

        private void LogToFile(string toLog, LogLevel type)
        {
            try
            {
                File.AppendAllText(FileName, $"[{type}] {DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss", CultureInfo.CurrentCulture)}: {toLog}");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Logs a text string to the various recievers in this logger
        /// </summary>
        /// <param name="toLog">The pre-formatted string to log</param>
        /// <param name="type">The log level for this message</param>
        /// <param name="args">Any format arguments for the string</param>
        public void Log(string toLog, LogLevel type, params object[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            LogToFile(string.Format(CultureInfo.CurrentCulture, toLog, args), type);

            LogEntry newEntry = new()
            {
                Caller = Caller,
                Level = type,
                Session = GUID,
                SessionStart = SessionStart
            };

            MessageBus?.Log(string.Format(CultureInfo.CurrentCulture, toLog, args), type);

            newEntry.Value = args.Length > 0 ? string.Format(CultureInfo.CurrentCulture, toLog, args) : toLog;

            newEntry.DateCreated = DateTime.Now;

#if DEBUG
            System.Diagnostics.Debug.WriteLine("[" + DateTime.Now.ToString(CultureInfo.CurrentCulture) + "] " + newEntry.Value);
#else

#endif

            Entries.Add(newEntry);
        }

        /// <summary>
        /// Logs a debug level message to the various recievers
        /// </summary>
        /// <param name="toLog">The preformatted string to log</param>
        /// <param name="args">Optional arguments to be used during string formatting</param>
        public void LogDebug(string toLog, params object[] args)
        {
            Log(toLog, LogLevel.Debug, args);
        }

        /// <summary>
        /// Logs an error level message to the various recievers
        /// </summary>
        /// <param name="toLog">The preformatted string to log</param>
        /// <param name="args">Optional arguments to be used during string formatting</param>
        public void LogError(string toLog, params object[] args)
        {
            Log(toLog, LogLevel.Error, args);
        }

        /// <summary>
        /// Logs an exception as an exception level message to the various recievers
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public void LogException(Exception ex)
        {
            Contract.Requires(ex != null);

            LogError(ex.Message);
            MessageBus?.Log(ex);
            ErrorRepository.AddOrUpdate(new AuditableError(ex));
        }

        /// <summary>
        /// Logs an informational message to the various recievers
        /// </summary>
        /// <param name="toLog">The preformatted string to log</param>
        /// <param name="args">Optional arguments to be used during string formatting</param>
        public void LogInfo(string toLog, params object[] args)
        {
            Log(toLog, LogLevel.Info, args);
        }

        /// <summary>
        /// Logs an warning level message to the various recievers
        /// </summary>
        /// <param name="toLog">The preformatted string to log</param>
        /// <param name="args">Optional arguments to be used during string formatting</param>
        public void LogWarning(string toLog, params object[] args)
        {
            Log(toLog, LogLevel.Warning, args);
        }

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        /// Disposes of this logger and persists messages to the underlying database for IRepository implementations
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this logger and persists messages to the underlying database for IRepository implementations
        /// </summary>
        /// <param name="disposing">unused</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                LogEntryRepository.AddOrUpdateRange(Entries);

                LogEntryRepository.Commit(WriteContext);

                disposedValue = true;
            }

            WriteContext.Dispose();
        }
    }
}
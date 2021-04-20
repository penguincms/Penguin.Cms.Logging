using Penguin.Cms.Logging.Entities;
using Penguin.Persistence.Abstractions.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Logging.Extensions
{
    public static class LogEntryRepositoryExtensions
    {
        /// <summary>
        /// Gets all messages logged by a given caller (Type.ToString())
        /// </summary>
        /// <param name="caller">The Type.ToString() for the object that did the logging</param>
        /// <returns>All messages requested by the given caller</returns>
        public static List<LogEntry> GetByCaller(this IRepository<LogEntry> repository, string caller)
        {
            return repository.Where(l => l.Caller == caller).ToList();
        }

        /// <summary>
        /// Gets all messages logged as part of a single session
        /// </summary>
        /// <param name="session">The session id for the messages to be retrieved</param>
        /// <returns>All the messages logged as part of this session</returns>
        public static List<LogEntry> GetBySession(this IRepository<LogEntry> repository, string session)
        {
            return repository.Where(l => l.Session == session).ToList();
        }

        /// <summary>
        /// Gets a list of all the distinct options for callers of the message logger
        /// To use as parameters in GetByCaller
        /// </summary>
        /// <returns>A list of all distinct callers of the message logger</returns>
        public static List<string> GetDistinctCallers(this IRepository<LogEntry> repository)
        {
            return repository.Select(c => c.Caller).Distinct().ToList();
        }
    }
}
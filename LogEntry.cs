using Penguin.Auditing.Abstractions.Attributes;
using Penguin.Cms.Entities;
using Penguin.Messaging.Logging;
using Penguin.Persistence.Abstractions.Attributes.Validation;
using System;

namespace Penguin.Cms.Logging.Entities
{
    /// <summary>
    /// Represents an individual logged message
    /// </summary>
    [DontAuditChanges]
    public class LogEntry : Entity
    {
        /// <summary>
        /// The Type.ToString of the object that requested the logger managing this entry
        /// </summary>
        [MaxLength(100)]
        public string Caller { get; set; }

        /// <summary>
        /// The level that this message was logged at
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The string ID of the session for this logging entry
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// The time that this session was initialized
        /// </summary>
        public DateTime SessionStart { get; set; }

        /// <summary>
        /// The text value of the body of this log entry
        /// </summary>
        public string Value { get; set; }
    }
}
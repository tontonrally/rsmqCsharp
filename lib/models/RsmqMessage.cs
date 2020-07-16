using System;

namespace RsmqCsharp
{
    public class RsmqMessage
    {
        /// <summary>
        /// The message's contents.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The internal message id.
        /// </summary>
        [RsmqIdAttribute]
        public string Id { get; set; }
        /// <summary>
        /// DateTime of when this message was sent / created.
        /// </summary>
        public DateTime Sent { get; set; }
        /// <summary>
        /// DateTime of when this message was first received.
        /// </summary>
        public DateTime FirstReceived { get; set; }
        /// <summary>
        /// Number of times this message was received.
        /// </summary>
        public int ReceivedCount { get; set; }
    }
}
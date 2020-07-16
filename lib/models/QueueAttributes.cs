using System;

namespace RsmqCsharp
{
    public class QueueAttributes
    {
        /// <summary>
        /// The visibility timeout for the queue in seconds
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int VisibilityTimer { get; set; }
        /// <summary>
        /// The delay for new messages in seconds
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int Delay { get; set; }
        /// <summary>
        /// The maximum size of a message in bytes
        /// </summary>
        [RsmqMaxSizeAttribute]
        public int MaxSize { get; set; }
        /// <summary>
        /// Total number of messages received from the queue
        /// </summary>
        public int TotalReceived { get; set; }
        /// <summary>
        /// Total number of messages sent to the queue
        /// </summary>
        public int TotalSent { get; set; }
        /// <summary>
        /// DateTime when the queue was created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// DateTime when the queue was last modified with SetQueueAttributes
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// Current number of messages in the queue
        /// </summary>
        public int Messages { get; set; }
        /// <summary>
        /// Current number of hidden / not visible messages. A message can be hidden while "in flight" due to a VisibilityTimer parameter or when sent with a Delay
        /// </summary>
        public int HiddenMessages { get; set; }
    }
}
namespace RsmqCsharp
{
    public class SetQueueAttributesOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
        /// <summary>
        /// (optional) The length of time, in seconds, that a message received from a queue will be invisible to other receiving components when they ask to receive messages. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int? VisibilityTimer { get; set; }
        /// <summary>
        /// (optional) The time in seconds that the delivery of all new messages in the queue will be delayed. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int? Delay { get; set; }
        /// <summary>
        /// (optional) The maximum message size in bytes. Allowed values: 1024-65536 and -1 (for unlimited size)
        /// </summary>
        [RsmqMaxSizeAttribute]
        public int? MaxSize { get; set; }
    }
}
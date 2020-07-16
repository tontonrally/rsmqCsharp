namespace RsmqCsharp
{
    public class SendMessageOptions
    {
        /// <summary>
        /// The queue name
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
        /// <summary>
        /// The message
        /// </summary>
        [RsmqMessageAttribute]
        public string Message { get; set; }
        /// <summary>
        /// (optional) (Default: queue settings) The time in seconds that the delivery of the message will be delayed. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int? Delay { get; set; }
    }
}
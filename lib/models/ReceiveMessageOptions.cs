namespace RsmqCsharp
{
    public class ReceiveMessageOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
        /// <summary>
        /// (optional) (Default: queue settings) The length of time, in seconds, that the received message will be invisible to others. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int? VisibilityTimer { get; set; }
    }
}
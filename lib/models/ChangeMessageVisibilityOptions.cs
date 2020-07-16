namespace RsmqCsharp
{
    public class ChangeMessageVisibilityOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
        /// <summary>
        /// The message id.
        /// </summary>
        [RsmqIdAttribute]
        public string Id { get; set; }
        /// <summary>
        /// The length of time, in seconds, that this message will not be visible. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int VisibilityTimer { get; set; }
    }
}
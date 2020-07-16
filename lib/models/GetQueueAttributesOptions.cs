namespace RsmqCsharp
{
    public class GetQueueAttributesOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
    }
}
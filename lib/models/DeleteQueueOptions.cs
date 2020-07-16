namespace RsmqCsharp
{
    public class DeleteQueueOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        /// <value></value>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
    }
}
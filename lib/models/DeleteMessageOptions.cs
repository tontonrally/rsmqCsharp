namespace RsmqCsharp
{
    public class DeleteMessageOptions
    {
        /// <summary>
        /// The Queue name.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
        /// <summary>
        /// message id to delete.
        /// </summary>
        [RsmqIdAttribute]
        public string Id { get; set; }
    }
}
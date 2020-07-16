namespace RsmqCsharp
{
    public class PopMessageOptions
    {
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }
    }
}
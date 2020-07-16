namespace RsmqCsharp
{
    public class CreateQueueOptions
    {
        /// <summary>
        /// The Queue name. Maximum 160 characters; alphanumeric characters, hyphens (-), and underscores (_) are allowed.
        /// </summary>
        [RsmqQueueNameAttribute]
        public string QueueName { get; set; }

        private int _visibilityTimer = 30;
        /// <summary>
        /// optional (Default: 30) The length of time, in seconds, that a message received from a queue will be invisible to other receiving components when they ask to receive messages. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int VisibilityTimer
        {
            get
            {
                return _visibilityTimer;
            }
            set
            {
                _visibilityTimer = value;
            }
        }

        private int _delay = 0;
        /// <summary>
        /// optional (Default: 0) The time in seconds that the delivery of all new messages in the queue will be delayed. Allowed values: 0-9999999 (around 115 days)
        /// </summary>
        [RsmqVisibilityTimerOrDelayAttribute]
        public int Delay
        {
            get
            {
                return _delay;
            }
            set
            {
                _delay = value;
            }
        }

        private int _maxSize = 65536;
        /// <summary>
        /// optional (Default: 65536) The maximum message size in bytes. Allowed values: 1024-65536 and -1 (for unlimited size)
        /// </summary>
        [RsmqMaxSizeAttribute]
        public int MaxSize
        {
            get
            {
                return _maxSize;
            }
            set
            {
                _maxSize = value;
            }
        }
    }
}
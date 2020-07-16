namespace RsmqCsharp
{
    public class RsmqQueue
    {
        [RsmqVisibilityTimerOrDelayAttribute]
        public int VisibilityTimer { get; set; }
        [RsmqVisibilityTimerOrDelayAttribute]
        public int Delay { get; set; }
        [RsmqMaxSizeAttribute]
        public int MaxSize { get; set; }
        public long Timestamp { get; set; }
        public string Uid { get; set; }
    }
}
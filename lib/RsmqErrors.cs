namespace RsmqCsharp
{
    internal static class RsmqErrors
    {
        public static string NoAttributeSupplied() => "No attribute was supplied";
        public static string MissingParameter(string item) => $"No {item} supplied";
        public static string InvalidFormat(string item) => $"Invalid {item} format";
		public static string InvalidValue(string item, int min, int max) => $"{item} must be between {min} and {max}";
		public static string MessageNotString() => "Message must be a string";
		public static string MessageTooLong() => "Message too long";
		public static string QueueNotFound() => "Queue not found";
		public static string QueueExists() => "Queue exists";
    }
}
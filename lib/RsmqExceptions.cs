using System;

namespace RsmqCsharp
{
    public class NoAttributeSuppliedException : Exception
    {
        public NoAttributeSuppliedException() : base(RsmqErrors.NoAttributeSupplied()) { }
    }

    public class MissingParameterException : Exception
    {
        public MissingParameterException(string item) : base(RsmqErrors.MissingParameter(item)) { }
    }

    public class InvalidFormatException : Exception
    {
        public InvalidFormatException(string item) : base(RsmqErrors.InvalidFormat(item)) { }
    }

    public class InvalidValueException : Exception
    {
        public InvalidValueException(string item, int min, int max) : base(RsmqErrors.InvalidValue(item, min, max)) { }
    }

    public class MessageTooLongException : Exception
    {
        public MessageTooLongException() : base(RsmqErrors.MessageTooLong()) { }
    }

    public class QueueNotFoundException : Exception
    {
        public QueueNotFoundException() : base(RsmqErrors.QueueNotFound()) { }
    }

    public class QueueExistsException : Exception
    {
        public QueueExistsException() : base(RsmqErrors.QueueExists()) { }
    }
}
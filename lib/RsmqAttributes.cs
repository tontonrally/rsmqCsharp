using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RsmqCsharp
{
    public interface IRsmqAttribute
    {
        void IsValid(object value, System.Reflection.PropertyInfo propertyInfo);
    }

    public class RsmqQueueNameAttribute : Attribute, IRsmqAttribute
    {
        public void IsValid(object value, System.Reflection.PropertyInfo propertyInfo)
        {
            if (value != null)
            {
                string q = value.ToString();

                if (!Regex.IsMatch(q, @"([a-zA-Z0-9_-]){1,160}"))
                {
                    throw new InvalidFormatException(propertyInfo.Name);
                }
            }
            else
            {
                throw new MissingParameterException(propertyInfo.Name);
            }
        }
    }

    public class RsmqIdAttribute : Attribute, IRsmqAttribute
    {
        public void IsValid(object value, System.Reflection.PropertyInfo propertyInfo)
        {
            if (value != null)
            {
                string q = value.ToString();

                if (!Regex.IsMatch(q, @"([a-zA-Z0-9:]){32}"))
                {
                    throw new InvalidFormatException(propertyInfo.Name);
                }
            }
            else
            {
                throw new MissingParameterException(propertyInfo.Name);
            }
        }
    }

    public class RsmqVisibilityTimerOrDelayAttribute : Attribute, IRsmqAttribute
    {
        public void IsValid(object value, System.Reflection.PropertyInfo propertyInfo)
        {
            if (value != null)
            {
                int v = (int)value;

                if (!(v >= 0 && v <= 9999999))
                {
                    throw new InvalidValueException(propertyInfo.Name, 0, 9999999);
                }
            }
            else
            {
                throw new MissingParameterException(propertyInfo.Name);
            }
        }
    }

    public class RsmqMaxSizeAttribute : Attribute, IRsmqAttribute
    {
        public void IsValid(object value, System.Reflection.PropertyInfo propertyInfo)
        {
            if (value != null)
            {
                int v = (int)value;

                if (!(v == -1 || (v >= 1024 && v <= 65536)))
                {
                    throw new InvalidValueException(propertyInfo.Name, 1024, 65536);
                }
            }
            else
            {
                throw new MissingParameterException(propertyInfo.Name);
            }
        }
    }

    public class RsmqMessageAttribute : Attribute, IRsmqAttribute
    {
        public void IsValid(object value, System.Reflection.PropertyInfo propertyInfo)
        {
            if (string.IsNullOrEmpty(value.ToString()))
            {
                throw new MissingParameterException(propertyInfo.Name);
            }
        }
    }
}
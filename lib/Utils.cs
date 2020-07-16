using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace RsmqCsharp
{
    internal static class Utils
    {
        public static Dictionary<string, string> ExtractPropsFromRedisHashEntries(HashEntry[] values, string[] props)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                var entry = values.SingleOrDefault(qa => qa.Name.ToString() == prop);

                res.Add(prop, entry == null ? null : entry.Value.ToString());
            }

            return res;
        }

        public static string MakeId(int len)
        {
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var i = 0;
            string uid = "";
            var random = new Random();
            for (i = 0; i < len; i++)
            {
                uid += possible.ElementAt(random.Next(possible.Length));
            }

            return uid;
        }
    }
}
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RsmqCsharp
{
    public class LuaScripts
    {
        // The popMessage LUA Script
        //
        // Parameters:
        //
        // @key: the zset key
        // @timestamp: the current time in ms
        //
        // * Find a message id
        // * Get the message
        // * Increase the rc (receive count)
        // * Use hset to set the fr (first receive) time
        // * Return the message and the counters
        //
        // Returns:
        //
        // {id, message, rc, fr}
        internal static LuaScript PopMessageScript
        {
            get
            {
                string script = "";

                script += "local msg = redis.call(\"ZRANGEBYSCORE\", @key, \"-inf\", @timestamp, \"LIMIT\", \"0\", \"1\")\n";
                script += "if #msg == 0 then\n";
                script += "    return {}\n";
                script += "end\n";
                script += "redis.call(\"HINCRBY\", @key .. \":Q\", \"totalrecv\", 1)\n";
                script += "local mbody = redis.call(\"HGET\", @key .. \":Q\", msg[1])\n";
                script += "local rc = redis.call(\"HINCRBY\", @key .. \":Q\", msg[1] .. \":rc\", 1)\n";
                script += "local o = {msg[1], mbody, rc}\n";
                script += "if rc==1 then\n";
                script += "    table.insert(o, @timestamp)\n";
                script += "else\n";
                script += "    local fr = redis.call(\"HGET\", @key .. \":Q\", msg[1] .. \":fr\")\n";
                script += "    table.insert(o, fr)\n";
                script += "end\n";
                script += "redis.call(\"ZREM\", @key, msg[1])\n";
                script += "redis.call(\"HDEL\", @key .. \":Q\", msg[1], msg[1] .. \":rc\", msg[1] .. \":fr\")\n";
                script += "return o\n";

                return LuaScript.Prepare(script);
            }
        }

        // The receiveMessage LUA Script
        //
        // Parameters:
        //
        // @key: the zset key
        // @timestamp: the current time in ms
        // @timestampTimeout: the new calculated time when the vt runs out
        //
        // * Find a message id
        // * Get the message
        // * Increase the rc (receive count)
        // * Use hset to set the fr (first receive) time
        // * Return the message and the counters
        //
        // Returns:
        //
        // {id, message, rc, fr}
        internal static LuaScript ReceiveMessageScript
        {
            get
            {
                string script = "";

                script += "local msg = redis.call(\"ZRANGEBYSCORE\", @key, \"-inf\", @timestamp, \"LIMIT\", \"0\", \"1\")\n";
                script += "if #msg == 0 then\n";
                script += "    return {}\n";
                script += "end\n";
                script += "redis.call(\"ZADD\", @key, @timestampTimeout, msg[1])\n";
                script += "redis.call(\"HINCRBY\", @key .. \":Q\", \"totalrecv\", 1)\n";
                script += "local mbody = redis.call(\"HGET\", @key .. \":Q\", msg[1])\n";
                script += "local rc = redis.call(\"HINCRBY\", @key .. \":Q\", msg[1] .. \":rc\", 1)\n";
                script += "local o = {msg[1], mbody, rc}\n";
                script += "if rc==1 then\n";
                script += "    redis.call(\"HSET\", @key .. \":Q\", msg[1] .. \":fr\", @timestamp)\n";
                script += "    table.insert(o, @timestamp)\n";
                script += "else\n";
                script += "    local fr = redis.call(\"HGET\", @key .. \":Q\", msg[1] .. \":fr\")\n";
                script += "    table.insert(o, fr)\n";
                script += "end\n";
                script += "return o\n";

                return LuaScript.Prepare(script);
            }
        }

        // The changeMessageVisibility LUA Script
        //
        // Parameters:
        //
        // @key: the zset key
        // @id: the message id
        // @newTimer: the new timer to set
        //
        // * Find the message id
        // * Set the new timer
        //
        // Returns:
        //
        // 0 or 1
        internal static LuaScript ChangeMessageVisibilityScript
        {
            get
            {
                string script = "";

                script += "local msg = redis.call(\"ZSCORE\", @key, @id)\n";
                script += "if not msg then\n";
                script += "	return 0\n";
                script += "end\n";
                script += "redis.call(\"ZADD\", @key, @newTimer, @id)\n";
                script += "return 1\n";

                return LuaScript.Prepare(script);
            }
        }
    }
}
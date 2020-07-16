using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Base36Library;
using System.ComponentModel.DataAnnotations;

namespace RsmqCsharp
{
    public class Rsmq
    {
        private RsmqOptions _options { get; set; }
        private RedisConnectionPool _connection { get; set; }

        private LoadedLuaScript _receiveMessage = null;
        private LoadedLuaScript _popMessage = null;
        private LoadedLuaScript _changeMessageVisibility = null;

        private IDatabase RedisClient
        {
            get
            {
                return _connection.GetConnection().GetDatabase(0);
            }
        }

        private IServer RedisServer
        {
            get
            {
                return _connection.GetConnection().GetServer(this._options.Host, this._options.Port);
            }
        }

        private async Task<DateTime> GetServerTime()
        {
            return await RedisServer.TimeAsync();
        }

        private async Task<RsmqQueue> GetQueue(string queueName, bool uid = false)
        {
            var transaction = RedisClient.CreateTransaction();

            var queueAttrTask = transaction.HashGetAllAsync($"{this._options.Namespace}:{queueName}:Q");
            var time = await GetServerTime();

            if (await transaction.ExecuteAsync())
            {
                var queueAttr = await queueAttrTask;

                if (queueAttr.Length == 0)
                {
                    throw new QueueNotFoundException();
                }

                var props = Utils.ExtractPropsFromRedisHashEntries(queueAttr, new string[] { "vt", "delay", "maxsize" });

                var q = new RsmqQueue
                {
                    VisibilityTimer = int.Parse(props["vt"]),
                    Delay = int.Parse(props["delay"]),
                    MaxSize = int.Parse(props["maxsize"]),
                    Timestamp = ((DateTimeOffset)time).ToUnixTimeMilliseconds()
                };

                if (uid)
                {
                    var us = time.ToString("ffffff");

                    var b36Id = Base36.Encode(long.Parse($"{((DateTimeOffset)time).ToUnixTimeSeconds()}{us}"));
                    q.Uid = $"{b36Id}{Utils.MakeId(22)}";
                }

                return q;
            }

            return null;
        }

        private void Validate(object model, string propName = null)
        {
            Validate(model, propName != null ? new string[] { propName } : null);
        }

        private void Validate(object model, string[] propNames)
        {
            foreach (var propertyInfo in model.GetType().GetProperties())
            {
                foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                {
                    var attr = attribute as IRsmqAttribute;
                    if (attr != null)
                    {
                        if (propNames == null || propNames.Contains(propertyInfo.Name))
                        {
                            attr.IsValid(propertyInfo.GetValue(model), propertyInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance of RSMQ.
        /// </summary>
        public Rsmq(RsmqOptions options = null)
        {
            this._options = options ?? new RsmqOptions();

            this._connection = new RedisConnectionPool($"{this._options.Host}:{this._options.Port}{(this._options.Options != null ? $",{this._options.Options.TrimStart(',')}" : "")}");

            _receiveMessage = LuaScripts.ReceiveMessageScript.Load(this.RedisServer);
            _popMessage = LuaScripts.PopMessageScript.Load(this.RedisServer);
            _changeMessageVisibility = LuaScripts.ChangeMessageVisibilityScript.Load(this.RedisServer);
        }

        /// <summary>
        /// Synchronously create a new queue
        /// </summary>
        public int CreateQueue(CreateQueueOptions options)
        {
            return CreateQueueAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously create a new queue.
        /// </summary>
        public async Task<int> CreateQueueAsync(CreateQueueOptions options)
        {
            var committed = false;
            var key = $"{_options.Namespace}:{options.QueueName}:Q";

            Validate(options);

            try
            {
                var time = await GetServerTime();

                var transaction = RedisClient.CreateTransaction();

                var tasks = new List<Task>();
                tasks.Add(transaction.HashSetAsync(key, "vt", options.VisibilityTimer, When.NotExists));
                tasks.Add(transaction.HashSetAsync(key, "delay", options.Delay, When.NotExists));
                tasks.Add(transaction.HashSetAsync(key, "maxsize", options.MaxSize, When.NotExists));
                tasks.Add(transaction.HashSetAsync(key, "created", ((DateTimeOffset)time).ToUnixTimeSeconds(), When.NotExists));
                tasks.Add(transaction.HashSetAsync(key, "modified", ((DateTimeOffset)time).ToUnixTimeSeconds(), When.NotExists));

                committed = await transaction.ExecuteAsync();

                if (committed)
                {
                    Task.WaitAll(tasks.ToArray());

                    await RedisClient.SetAddAsync($"{this._options.Namespace}:QUEUES", options.QueueName);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                committed = false;
            }

            if (!committed)
            {
                throw new QueueExistsException();
            }

            return committed ? 1 : 0;
        }

        /// <summary>
        /// Synchronously list all queues
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ListQueues()
        {
            return ListQueuesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously list all queues
        /// </summary>
        public async Task<IEnumerable<string>> ListQueuesAsync()
        {
            var queues = await RedisClient.SetMembersAsync($"{this._options.Namespace}:QUEUES");
            return queues.Select(q => q.ToString());
        }

        /// <summary>
        /// Synchronously deletes a queue and all messages.
        /// </summary>
        public int DeleteQueue(DeleteQueueOptions options)
        {
            return DeleteQueueAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously deletes a queue and all messages.
        /// </summary>
        public async Task<int> DeleteQueueAsync(DeleteQueueOptions options)
        {
            Validate(options);

            var committed = false;
            var key = $"{_options.Namespace}:{options.QueueName}";

            try
            {
                var transaction = RedisClient.CreateTransaction();

                var tasks = new List<Task>
                {
                    transaction.HashDeleteAsync($"{key}:Q", key),
                    transaction.SetRemoveAsync($"{this._options.Namespace}:QUEUES", options.QueueName)
                };

                committed = await transaction.ExecuteAsync();

                if (committed)
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                committed = false;
            }

            return committed ? 1 : 0;
        }

        /// <summary>
        /// Synchronously get queue attributes, counter and stats
        /// </summary>
        public QueueAttributes GetQueueAttributes(GetQueueAttributesOptions options)
        {
            return GetQueueAttributesAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously get queue attributes, counter and stats
        /// </summary>
        public async Task<QueueAttributes> GetQueueAttributesAsync(GetQueueAttributesOptions options)
        {
            Validate(options);
            var key = $"{_options.Namespace}:{options.QueueName}";
            var time = await GetServerTime();

            var transaction = RedisClient.CreateTransaction();

            var queueAttrTask = transaction.HashGetAllAsync($"{key}:Q");
            var msgTask = transaction.SortedSetLengthAsync(key);
            var hiddemMsgTask = transaction.SortedSetLengthByValueAsync(key, ((DateTimeOffset)time).ToUnixTimeMilliseconds(), "+inf");

            if (await transaction.ExecuteAsync())
            {
                var queueAttr = await queueAttrTask;
                var msg = await msgTask;
                var hiddemMsg = await hiddemMsgTask;

                if (queueAttr.Length == 0)
                {
                    throw new QueueNotFoundException();
                }

                var props = Utils.ExtractPropsFromRedisHashEntries(queueAttr, new string[] { "vt", "delay", "maxsize", "totalrecv", "totalsent", "created", "modified" });

                return new QueueAttributes
                {
                    VisibilityTimer = int.Parse(props["vt"]),
                    Delay = int.Parse(props["delay"]),
                    MaxSize = int.Parse(props["maxsize"]),
                    TotalReceived = int.Parse(props["totalrecv"] ?? "0"),
                    TotalSent = int.Parse(props["totalsent"] ?? "0"),
                    Created = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(props["created"])).UtcDateTime,
                    Modified = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(props["modified"])).UtcDateTime,
                    Messages = (int)msg,
                    HiddenMessages = (int)hiddemMsg
                };
            }

            return null;
        }

        /// <summary>
        /// Synchronously sets queue parameters.
        /// </summary>
        public QueueAttributes SetQueueAttributes(SetQueueAttributesOptions options)
        {
            return SetQueueAttributesAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // <summary>
        /// Asynchronously sets queue parameters.
        /// </summary>
        public async Task<QueueAttributes> SetQueueAttributesAsync(SetQueueAttributesOptions options)
        {
            if (!options.VisibilityTimer.HasValue && !options.MaxSize.HasValue && !options.Delay.HasValue)
            {
                throw new NoAttributeSuppliedException();
            }

            var validateOptions = new List<string> { "QueueName" };

            if (options.VisibilityTimer.HasValue) validateOptions.Add("VisibilityTimer");
            if (options.MaxSize.HasValue) validateOptions.Add("MaxSize");
            if (options.Delay.HasValue) validateOptions.Add("Delay");

            Validate(options, validateOptions.ToArray());

            var key = $"{this._options.Namespace}:{options.QueueName}";

            var q = await this.GetQueue(options.QueueName);
            var time = await this.GetServerTime();

            var transaction = RedisClient.CreateTransaction();

            var tasks = new Task[] {
                transaction.HashSetAsync($"{key}:Q", "modified", ((DateTimeOffset)time).ToUnixTimeSeconds())
            };

            if (options.VisibilityTimer.HasValue)
            {
                tasks.Append(transaction.HashSetAsync($"{key}:Q", "vt", options.VisibilityTimer));
            }

            if (options.MaxSize.HasValue)
            {
                tasks.Append(transaction.HashSetAsync($"{key}:Q", "maxsize", options.MaxSize));
            }

            if (options.Delay.HasValue)
            {
                tasks.Append(transaction.HashSetAsync($"{key}:Q", "delay", options.Delay));
            }

            if (await transaction.ExecuteAsync())
            {
                Task.WaitAll(tasks);

                return await this.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = options.QueueName });
            }

            return null;
        }

        /// <summary>
        /// Synchronously sends a new message.
        /// </summary>
        /// <returns>
        /// The internal message id
        /// </returns>
        public string SendMessage(SendMessageOptions options)
        {
            return SendMessageAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously sends a new message.
        /// </summary>
        /// <returns>
        /// The internal message id
        /// </returns>
        public async Task<string> SendMessageAsync(SendMessageOptions options)
        {
            try
            {
                Validate(options, "QueueName");
                var q = await GetQueue(options.QueueName, true);

                options.Delay = options.Delay.HasValue ? options.Delay.Value : q.Delay;
                Validate(options);

                if (q.MaxSize != -1 && options.Message.Length > q.MaxSize)
                {
                    throw new MessageTooLongException();
                }

                var key = $"{this._options.Namespace}:{options.QueueName}";

                var transaction = RedisClient.CreateTransaction();

                var tasks = new Task[] {
                    transaction.SortedSetAddAsync(key, q.Uid, q.Timestamp + (int)options.Delay * 1000),
                    transaction.HashSetAsync($"{key}:Q", q.Uid, options.Message),
                    transaction.HashIncrementAsync($"{key}:Q", "totalsent", 1)
                };

                Task<long> rtTask = null;
                if (this._options.Realtime)
                {
                    rtTask = transaction.SortedSetLengthAsync(key);
                }

                if (await transaction.ExecuteAsync())
                {
                    Task.WaitAll(tasks);
                    if (this._options.Realtime && rtTask != null)
                    {
                        var sslength = await rtTask;
                        await RedisClient.PublishAsync($"{this._options.Namespace}rt:{options.QueueName}", sslength);
                    }

                    return q.Uid;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            return null;
        }

        /// <summary>
        /// Synchronously receive the next message from the queue.
        /// </summary>
        public RsmqMessage ReceiveMessage(ReceiveMessageOptions options)
        {
            return ReceiveMessageAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously receive the next message from the queue.
        /// </summary>
        public async Task<RsmqMessage> ReceiveMessageAsync(ReceiveMessageOptions options)
        {
            Validate(options, "QueueName");
            var q = await this.GetQueue(options.QueueName);

            options.VisibilityTimer = options.VisibilityTimer.HasValue ? options.VisibilityTimer.Value : q.VisibilityTimer;
            Validate(options);

            var res = await _receiveMessage.EvaluateAsync(
                this.RedisClient,
                new
                {
                    key = (RedisKey)$"{this._options.Namespace}:{options.QueueName}",
                    timestamp = q.Timestamp,
                    timestampTimeout = q.Timestamp + options.VisibilityTimer * 1000
                }
                );

            var vals = (string[])res;

            if (vals.Length != 4)
            {
                return null;
            }

            return new RsmqMessage
            {
                Id = vals[0],
                Message = vals[1],
                ReceivedCount = int.Parse(vals[2]),
                FirstReceived = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(vals[3])).UtcDateTime,
                Sent = DateTimeOffset.FromUnixTimeMilliseconds(Base36.Decode(vals[0].Substring(0, 10)) / 1000).UtcDateTime
            };
        }

        /// <summary>
        /// Synchronously deletes a message
        /// 
        /// returns 1 if successful, 0 if the message was not found
        /// </summary>
        public int DeleteMessage(DeleteMessageOptions options)
        {
            return DeleteMessageAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously deletes a message
        /// 
        /// returns 1 if successful, 0 if the message was not found
        /// </summary>
        public async Task<int> DeleteMessageAsync(DeleteMessageOptions options)
        {
            Validate(options);
            var key = $"{this._options.Namespace}:{options.QueueName}";

            var transaction = this.RedisClient.CreateTransaction();

            var removeSortedSetTask = transaction.SortedSetRemoveAsync(key, options.Id);
            var removeHashTask = transaction.HashDeleteAsync($"{key}:Q", new RedisValue[] { options.Id, $"{options.Id}:rc", $"{options.Id}:fr" });

            if (await transaction.ExecuteAsync())
            {
                var removeSortedSetTaskResult = await removeSortedSetTask;
                var removeHashTaskResult = await removeHashTask;

                if (removeSortedSetTaskResult && removeHashTaskResult > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// Synchronously receive the next message from the queue and delete it.
        /// 
        /// Important: This method deletes the message it receives right away. There is no way to receive the message again if something goes wrong while working on the message.
        /// 
        /// returns null if no message is there
        /// </summary>
        public RsmqMessage PopMessage(PopMessageOptions options)
        {
            return PopMessageAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously receive the next message from the queue and delete it.
        /// 
        /// Important: This method deletes the message it receives right away. There is no way to receive the message again if something goes wrong while working on the message.
        /// 
        /// returns null if no message is there
        /// </summary>
        public async Task<RsmqMessage> PopMessageAsync(PopMessageOptions options)
        {
            // todo: validate (qname)
            var key = $"{this._options.Namespace}:{options.QueueName}";
            var q = await this.GetQueue(options.QueueName);

            var res = await _popMessage.EvaluateAsync(
                this.RedisClient,
                new
                {
                    key = key,
                    timestamp = q.Timestamp
                }
            );

            var vals = (string[])res;

            if (vals.Length != 4)
            {
                return null;
            }

            return new RsmqMessage
            {
                Id = vals[0],
                Message = vals[1],
                ReceivedCount = int.Parse(vals[2]),
                FirstReceived = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(vals[3])).UtcDateTime,
                Sent = DateTimeOffset.FromUnixTimeMilliseconds(Base36.Decode(vals[0].Substring(0, 10)) / 1000).UtcDateTime
            };
        }

        /// <summary>
        /// Synchronously change the visibility timer of a single message. The time when the message will be visible again is calculated from the current time (now) + VisibilityTimer.
        /// 
        /// returns 1 if successful, 0 if the message was not found
        /// </summary>
        public int ChangeMessageVisibility(ChangeMessageVisibilityOptions options)
        {
            return ChangeMessageVisibilityAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously change the visibility timer of a single message. The time when the message will be visible again is calculated from the current time (now) + VisibilityTimer.
        /// 
        /// returns 1 if successful, 0 if the message was not found
        /// </summary>
        public async Task<int> ChangeMessageVisibilityAsync(ChangeMessageVisibilityOptions options)
        {
            // todo : validate (qname, id, vt)
            var key = $"{this._options.Namespace}:{options.QueueName}";
            var q = await this.GetQueue(options.QueueName);

            var res = await _changeMessageVisibility.EvaluateAsync(
                this.RedisClient,
                new
                {
                    key = key,
                    id = options.Id,
                    newTimer = q.Timestamp + options.VisibilityTimer * 1000
                }
            );

            return (int)res;
        }

        /// <summary>
        /// Disconnect the redis client.
        /// </summary>
        public void Quit()
        {
            this._connection.Dispose();
        }
    }
}

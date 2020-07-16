using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RsmqCsharp
{
    internal class RedisConnectionPool : IRedisCacheConnectionPoolManager
    {
        private static ConcurrentBag<Lazy<ConnectionMultiplexer>> connections;
        private readonly string _redisConnectionString;
        private readonly int _poolSize;

        public RedisConnectionPool(string redisConnectionString, int poolSize = 1)
        {
            this._redisConnectionString = redisConnectionString;
            this._poolSize = poolSize;

            Initialize();
        }

        public IConnectionMultiplexer GetConnection()
        {
            Lazy<ConnectionMultiplexer> response;
            var loadedLazys = connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazys.Count() == connections.Count)
            {
                response = connections.OrderBy(x => x.Value.GetCounters().TotalOutstanding).First();
            }
            else
            {
                response = connections.First(lazy => !lazy.IsValueCreated);
            }

            return response.Value;
        }

        private void Initialize()
        {
            connections = new ConcurrentBag<Lazy<ConnectionMultiplexer>>();

            for (int i = 0; i < this._poolSize; i++)
            {
                connections.Add(new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this._redisConnectionString)));
            }
        }

        public void Dispose()
        {
            var activeConnections = connections.Where(lazy => lazy.IsValueCreated).ToList();
            activeConnections.ForEach(connection => connection.Value.Dispose());
            Initialize();
        }

        public ConnectionPoolInformation GetConnectionInformations()
        {
            throw new NotImplementedException();
        }
    }
}
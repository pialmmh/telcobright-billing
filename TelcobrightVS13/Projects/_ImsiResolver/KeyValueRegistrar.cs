using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace Imsi
{
    public class KeyValueRegistrar
    {
        private ConnectionMultiplexer redis;
        public IDatabase redisDb;

        public KeyValueRegistrar(String redisConnectionString)
        {
            redis = ConnectionMultiplexer.Connect(redisConnectionString);
            redisDb = redis.GetDatabase();
        }

        public void LoadToRedis(List<KeyValuePair<string, string>> batch)
        {
            var tran = redisDb.CreateTransaction();
            foreach (var pair in batch)
            {
                tran.StringSetAsync(pair.Key, pair.Value);
            }
            tran.Execute();
        }

        

    }
}

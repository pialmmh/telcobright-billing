using System;
using StackExchange.Redis;
using System.Collections.Generic;
using MediationModel;

namespace MemCache
{
    public class RedCache
    {
        private IDatabase _cache;
        private ConnectionMultiplexer _redis;

        public RedCache(string redisConnectionString)
        {
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _cache = _redis.GetDatabase();
        }

        // Method to add a key-value pair to Redis
        public void Add(string imsi, DateTime startTime, string phoneNumber)
        {
            // Generate Redis key as "imsi:starttime"
            var redisKey = GenerateRedisKey(imsi, startTime);

            // Check if key already exists
            if (!_cache.KeyExists(redisKey))
            {
                _cache.StringSet(redisKey, phoneNumber);
                //Console.WriteLine($"Added to Redis: {redisKey} => {phoneNumber}");
            }
            else
            {
                //Console.WriteLine($"Key {redisKey} already exists in Redis.");
            }
        }

        // Method to retrieve a value by key from Redis
        public string Get(string imsi, DateTime startTime)
        {
            // Generate Redis key as "imsi:starttime"
            var redisKey = GenerateRedisKey(imsi, startTime);

            // Fetch value (phone number) from Redis
            var phoneNumber = _cache.StringGet(redisKey);
            if (phoneNumber.HasValue)
            {
                return phoneNumber.ToString();
            }
            else
            {
                //Console.WriteLine($"Key {redisKey} not found in Redis.");
                return null;
            }
        }

        // Method to remove a key-value pair from Redis
        public void Remove(string imsi, DateTime startTime)
        {
            var redisKey = GenerateRedisKey(imsi, startTime);

            if (_cache.KeyExists(redisKey))
            {
                _cache.KeyDelete(redisKey);
                //Console.WriteLine($"Removed from Redis: {redisKey}");
            }
            else
            {
                //Console.WriteLine($"Key {redisKey} not found in Redis.");
            }
        }

        // Method to display all key-value pairs in Redis
        public void DisplayAll()
        {
            //Console.WriteLine("\nCurrent Redis Cache:");
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints[0]);

            foreach (var key in server.Keys(pattern: "*"))
            {
                var value = _cache.StringGet(key);
                //Console.WriteLine($"{key} => {value}");
            }
        }

        // Helper method to generate a unique Redis key "imsi:starttime"
        private string GenerateRedisKey(string imsi, DateTime startTime)
        {
            return $"{imsi}:{startTime:yyyy-MM-dd HH:mm:ss}";
        }

        // Method to fetch records in a time range
        public List<string[]> GetRecordsInTimeRange(DateTime startRange, DateTime endRange)
        {
            var result = new List<string[]>();

            // Get all keys in Redis
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints[0]);

            foreach (var key in server.Keys(pattern: "*"))
            {
                var redisKey = key.ToString();

                // Split the key into imsi and starttime
                var colonIndex = redisKey.IndexOf(':');
                if (colonIndex > 0)
                {
                    string startTimeString = redisKey.Substring(colonIndex + 1);
                    DateTime startTime = DateTime.Parse(startTimeString);
                    string imsi = redisKey.Substring(0, colonIndex);
                    string phoneNumber = _cache.StringGet(redisKey);
                    // Check if the startTime is within the provided range
                    if (startTime >= startRange && startTime <= endRange)
                    {
                        string[] sri = new string[150];
                        sri[Sn.StartTime] = startTime.ToString();
                        sri[Sn.Imsi] = imsi;
                        sri[Sn.TerminatingCalledNumber] = phoneNumber;
                        result.Add(sri);
                    }
                }
            }

            return result;
        }

        public void RemoveRecordsBeforeTime(DateTime endTime)
        {
            // Get all keys in Redis
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints[0]);

            foreach (var key in server.Keys(pattern: "*"))
            {
                var redisKey = key.ToString();

                // Split the key into imsi and starttime
                var colonIndex = redisKey.IndexOf(':');
                if (colonIndex > 0)
                {
                    string imsi = redisKey.Substring(0, colonIndex);  // IMSI is before the first colon
                    string startTimeString = redisKey.Substring(colonIndex + 1);  // StartTime is after the first colon
                    DateTime startTime = DateTime.Parse(startTimeString);

                    // Check if the startTime is before the provided endTime
                    if (startTime < endTime)
                    {
                        // Remove the record from Redis
                        _cache.KeyDelete(redisKey);
                        //Console.WriteLine($"Removed from Redis: {redisKey}");
                    }
                }
            }
        }
    }
}

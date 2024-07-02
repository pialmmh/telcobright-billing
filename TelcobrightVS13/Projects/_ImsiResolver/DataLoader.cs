using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text;
namespace Imsi
{
    public class ImsiResolver : KeyValueRegistrar
    {
        
        private MySqlConnection conn;
        KeyValueRegistrar registrar;
        private const int BatchSize = 1000000;

        public ImsiResolver(String mySqlProps, String redisConnectionString): base(redisConnectionString) 
        {
            conn = new MySqlConnection(mySqlProps);
            //registrar = new KeyValueRegistrar(redisConnectionString);
        }


        public void Init()
        {
            try
            {
                conn.Open();
                int offset = 0;
                while (true)
                {
                    List<KeyValuePair<string, string>> batch = LoadBatch(offset, BatchSize);
                    if (batch.Count == 0) break;
                    //registrar.LoadToRedis(batch);
                    LoadToRedis(batch);
                    offset += BatchSize;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }

        }

        private List<KeyValuePair<string, string>> LoadBatch(int offset, int limit)
        {
            List<KeyValuePair<string, string>> batch = new List<KeyValuePair<string, string>>();
            string query = "SELECT imsi, phoneNumber FROM imsi LIMIT @limit, @offset";//-----------<<<<<<<<<<query
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@limit", limit);
                cmd.Parameters.AddWithValue("@offset", offset);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string imsi = reader.GetString("imsi");
                        string phoneNumber = reader.GetString("phoneNumber");
                        batch.Add(new KeyValuePair<string, string>(imsi, phoneNumber));
                    }
                }
            }
            return batch;
        }

        public void AddData(List<KeyValuePair<string,string>> imsiVsPhoneNumbers)
        {
            try
            {
                conn.Open();
                string insertHeader = "INSERT INTO imsi (imsi, phoneNumber) VALUES \n";
                List<string> insertValues = new List<string>();

                imsiVsPhoneNumbers.ForEach(kv => {
                    redisDb.StringSet(kv.Key, kv.Value);
                    insertValues.Add($" ('{kv.Key}','{kv.Value}')");
                });
                StringBuilder query = new StringBuilder(insertHeader).Append(string.Join(",", insertValues)).Append(";");

                using (MySqlCommand cmd = new MySqlCommand(query.ToString(), conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                //registrar.redisDb.StringSet(imsi, phoneNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Found: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }


        public string GetData(string imsi)
        {
            try
            {
                //return registrar.redisDb.StringGet(imsi);
                return redisDb.StringGet(imsi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }


        public void DeleteData(string imsi)
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM imsi WHERE imsi = @imsi";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@imsi", imsi);
                    cmd.ExecuteNonQuery();
                }

                //registrar.redisDb.KeyDelete(imsi);
                redisDb.KeyDelete(imsi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Foundd: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }
    }




}


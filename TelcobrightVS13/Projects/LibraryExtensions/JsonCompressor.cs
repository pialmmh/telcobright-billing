using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO.Compression;

namespace LibraryExtensions
{
    public class JsonCompressor<T> where T:class 
    {
        public string SerializeToCompressedBase64String(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] inputBytes = Encoding.UTF8.GetBytes(json);
            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);

                var outputBytes = outputStream.ToArray();
                var outputbase64 = Convert.ToBase64String(outputBytes);
                return outputbase64;
            }
        }
        public T DeSerializeToObject(string compresedString)
        {
            byte[] inputBytes = Convert.FromBase64String(compresedString);
            string decompressedString;
            using (var inputStream = new MemoryStream(inputBytes))
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gZipStream.CopyTo(outputStream);
                var outputBytes = outputStream.ToArray();
                decompressedString = Encoding.UTF8.GetString(outputBytes);
            }
            return JsonConvert.DeserializeObject<T>(decompressedString);
        }
    }
}

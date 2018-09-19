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
        public string SerializeToCompressedBase64(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return CompressToGzipBase64(json);
        }

        public static string CompressToGzipBase64(string str)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);
            byte[] outputBytes = null;
            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);
                outputBytes = outputStream.ToArray();
            }
            var outputbase64 = Convert.ToBase64String(outputBytes);
            return outputbase64;
        }

        public T DeSerializeToObject(string compresedString)
        {
            string decompressedString = DeCompressFromGzipBase64(compresedString);
            return JsonConvert.DeserializeObject<T>(decompressedString);
        }

        public static string DeCompressFromGzipBase64(string compresedString)
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
            return decompressedString;
        }
    }
}

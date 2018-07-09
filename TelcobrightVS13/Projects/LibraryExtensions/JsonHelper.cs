using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LibraryExtensions
{
    public static class JsonHelper
    {
        public static void SerializeToFile<T>(T obj, string targetFileName) where T : class
        {
            using (FileStream fs = File.Open(targetFileName, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jw, obj);
            }
        }
        public static T DeSerializeFromFile<T>(string sourceFileName) where T:class
        {
            T obj=null;
            using (StreamReader file = File.OpenText(sourceFileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                obj = (T)serializer.Deserialize(file, typeof(T));
            }
            return obj;
        }
    }
}

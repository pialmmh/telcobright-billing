using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Decoders.SigtranDecoderHelper
{
    class JsonStringReader
    {
        public static List<string> ReadJsonObjects(string filePath)
        {
            List<string> jsonObjects = new List<string>();

            // Create a StringBuilder instance to build the current JSON object
            StringBuilder currentObjectBuilder = new StringBuilder();
            bool isInsideObject = false;
            int braceCount = 0;
            StringBuilder leftoverChunk = new StringBuilder(); // Store any leftover data from the previous chunk

            const int chunkSize = 200 * 1024 * 1024; // 200 MB chunk size (adjust as needed)

            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    // Get the length of the memory-mapped file
                    long fileSize = new FileInfo(filePath).Length;

                    byte[] buffer = new byte[chunkSize];
                    StringBuilder stringBuilder = new StringBuilder();
                    for (long offset = 0; offset < fileSize; offset += chunkSize)
                    {
                        int bytesToRead = (int)Math.Min(chunkSize, fileSize - offset);
                        accessor.ReadArray(offset, buffer, 0, bytesToRead);

                        // Convert the bytes read to a string and append to the StringBuilder
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesToRead));

                        // Combine the current chunk with any leftover data from the previous chunk
                        string chunk = leftoverChunk.Length != 0 ? leftoverChunk.Append(stringBuilder).ToString() : stringBuilder.ToString();
                        leftoverChunk.Clear();
                        if (chunk.Length > 0 && chunk[chunk.Length - 1] != '\n')
                        {
                            leftoverChunk.Append(chunk.Substring(chunk.LastIndexOf('\n') + 1)); // Store the data after the last newline
                            chunk = chunk.Substring(0, chunk.LastIndexOf('\n') + 1);
                        }
                        stringBuilder.Clear(); // Clear the builder after processing the chunk

                        chunk = ReplaceCurlyBracesInGsmSmsText(chunk);

                        // Process the combined chunk
                        for (int i = 0; i < chunk.Length; i++)
                        {
                            if (isInsideObject)
                            {
                                currentObjectBuilder.Append(chunk[i]);

                                if (chunk[i] == '{')
                                {
                                    braceCount++;
                                }
                                else if (chunk[i] == '}')
                                {
                                    braceCount--;
                                }

                                // If braceCount is 0, we've reached the end of the current JSON object
                                if (braceCount == 0)
                                {
                                    //jsonObjects.Add("{" + currentObjectBuilder.ToString());
                                    jsonObjects.Add(currentObjectBuilder.ToString());
                                    currentObjectBuilder.Clear();
                                    isInsideObject = false;
                                }
                            }
                            else
                            {
                                if (chunk[i] == '"' && chunk.Substring(i).StartsWith("\"layers\": {"))
                                {
                                    isInsideObject = true;
                                    currentObjectBuilder.Append(chunk.Substring(i, 10)); // Append "layers": {
                                    i += 9; // Move the index forward to the end of "layers": {
                                    braceCount = 1;
                                }
                            }
                        }
                    }
                }
            }

            // Check if there was an unclosed JSON object
            if (isInsideObject)
            {
                throw new InvalidDataException("The JSON file contains an unclosed object.");
            }

            // Remove "layers": { and corresponding closing } for each jsonObject
            List<string> cleanedJsonObjects = new List<string>();
            foreach (var jsonObject in jsonObjects)
            {
                //var js = jsonObject.TrimEnd('}');
                var cleanedObject = RemoveLayers(jsonObject);
                if (cleanedObject == null) continue;
                foreach (string obj in cleanedObject)
                {
                    cleanedJsonObjects.Add(obj);
                }
            }

            return cleanedJsonObjects;
        }

        static List<string> RemoveLayers(string jsonObject)
        {
            List<string> cleanedLines = new List<string>();
            bool skipLayersStart = false;
            bool skipLayersEnd = false;
            int braceCount = 0;

            int gsmMapCount = 0;

            using (StringReader reader = new StringReader(jsonObject))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("\"gsm_map\": {"))
                    {
                        gsmMapCount++;
                    }
                    if (line.Contains("\"layers\": {"))
                    {
                        skipLayersStart = true;
                        braceCount = 1;
                        continue;
                    }

                    if (skipLayersStart)
                    {
                        braceCount += line.Split('{').Length - 1;
                        braceCount -= line.Split('}').Length - 1;

                        if (braceCount == 0)
                        {
                            skipLayersEnd = true;
                            skipLayersStart = false;
                            continue;
                        }
                    }

                    if (skipLayersEnd)
                    {
                        skipLayersEnd = false;
                        continue;
                    }

                    cleanedLines.Add(line);
                }
            }



            //var objWithPossibleDup = cleanedObject.ToString();
            if (gsmMapCount > 1)
            {
                cleanedLines.Reverse();
                List<string> mapLines = cleanedLines.Skip(1).TakeWhile(l => l.Contains("tcap") == false).Reverse().ToList();
                mapLines = mapLines.SkipWhile(l => l.Contains("\"gsm_map\": {") == false).ToList();
                mapLines.First().TrimEnd(',');
                mapLines.Add("}");
                var printLine = String.Join("\n", mapLines) + "\n";
                List<string> mapObjectsAsStr = SeparateGsmMapObjects(mapLines);
                cleanedLines.Reverse();
                var linesWithoutGsmMap = cleanedLines.TakeWhile(l => l.Contains("\"gsm_map\": {") == false).ToList();

                List<String> separatedObjectsWithAllLayers = new List<string>();
                String messageHeader = String.Join("\n", linesWithoutGsmMap);
                foreach (string singleMapObj in mapObjectsAsStr)
                {
                    separatedObjectsWithAllLayers.Add("{\n" + messageHeader + "\n" + singleMapObj + "}\n");
                }
                var finalObjects = separatedObjectsWithAllLayers;
                return finalObjects;
            }
            else if (gsmMapCount == 1)
            {
                List<String> singleObjAsString = new List<string>();
                singleObjAsString.Add("{\n" + string.Join("\n", cleanedLines) + "\n}");
                return singleObjAsString;
            }
            else
            {
                return null;
            }

        }

        public static List<string> SeparateGsmMapObjects(List<string> mapLines)
        {
            List<string> jsonObjects = new List<string>();
            StringBuilder currentObject = null;
            bool insideGsmMap = false;
            int braceCount = 0;

            foreach (var line in mapLines)
            {
                if (line.Contains("\"gsm_map\": {") && !insideGsmMap)
                {
                    // Start a new gsm_map JSON object
                    currentObject = new StringBuilder();
                    insideGsmMap = true;
                    braceCount = 1;
                    currentObject.AppendLine(line);
                    continue;
                }

                if (insideGsmMap)
                {
                    braceCount += line.Split('{').Length - 1;
                    braceCount -= line.Split('}').Length - 1;
                    currentObject.AppendLine(line);
                    currentObject = currentObject.Replace("\r\n", "\n");
                    if (braceCount == 0)
                    {
                        // End of the gsm_map JSON object
                        insideGsmMap = false;
                        //jsonObjects.Add(currentObject.ToString().TrimEnd(','));
                        if (currentObject[currentObject.Length - 2] == ',') currentObject.Remove(currentObject.Length - 2, 1);
                        jsonObjects.Add(currentObject.ToString());
                        currentObject = null;
                    }
                }
            }

            if (insideGsmMap)
            {
                throw new InvalidDataException("Invalid JSON data: The last gsm_map object was not closed properly.");
            }

            return jsonObjects;
        }

        public static List<JObject> ParseJsonStrings(List<string> jsonStringList)
        {
            List<JObject> jobjectList = new List<JObject>();

            foreach (string jsonString in jsonStringList)
            {
                try
                {
                    JObject jobject = JObject.Parse(jsonString);
                    jobjectList.Add(jobject);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JSON string: {ex.Message}");
                }
            }

            return jobjectList;
        }

        static string ReplaceCurlyBracesInGsmSmsText(string chunk)
        {
            string pattern = "\"gsm_sms\\.sms_text\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"";
            return Regex.Replace(chunk, pattern, match =>
            {
                string text = match.Groups[1].Value;

                text = text.Replace("{", "OPENING_CURLY_BRACES").Replace("}", "ENDING_CURLY_BRACES");
                return $"\"gsm_sms.sms_text\": \"{text}\"";
            });
        }
    }
}

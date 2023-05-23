using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig
{
    public class InstanceMenu
    {
        public static List<string> getInstancesFromMenu(Dictionary<string,string> instances, string msgToDisplay)
        {
            Console.Clear();
            printMenu(instances, msgToDisplay);
            NameValueCollection configFiles = (NameValueCollection)ConfigurationManager.GetSection("appSettings");
            foreach (string key in configFiles)
            {
                
            }
            while (true)
            {
                List<int> userInputs = getUserInput();
                if (userInputs[0] == 0) // quit case
                {
                    return new List<string>();
                }
                else if (userInputs[0] < 0 || userInputs.Any(i=> i > instances.Count))// invalid case
                {
                    Console.Clear();
                    Console.WriteLine("Invalid input, try again...");
                    printMenu(instances,msgToDisplay);
                }
                else // valid case
                {
                    List<string> selectedKeys = userInputs.Select(i => "instance" + i.ToString()).ToList();
                    List<string> instanceNames = instances.Where(kv => selectedKeys.Contains(kv.Key))
                        .Select(kv => kv.Value).ToList();
                    Console.WriteLine("Selected instances:");
                    Console.WriteLine($"[{string.Join(",",instanceNames)}]");
                    return instanceNames;
                }
            }
        }
        static List<int> getUserInput()
        {
            string userInput = Console.ReadLine().Trim();
            List<int> finalInputs= new List<int>();
            if (userInput == "Q" || userInput == "q")
            {
                finalInputs.Add(0);
                return finalInputs;//0=quit
            }
            List<string> inputNumbersStr = userInput.Split(',').Select(inp=>inp.Trim()).ToList();
            foreach (string s in inputNumbersStr)
            {
                int inputAsNum = -1;
                bool successfullyParsed = int.TryParse(s, out inputAsNum);
                if (successfullyParsed && inputAsNum>0)
                {
                    finalInputs.Add(inputAsNum); //valid
                }
                else
                {
                    finalInputs = new List<int> {-1};// at least one input is invalid
                    return finalInputs;
                }
            }
            
            return finalInputs;
        }

        static void printMenu(Dictionary<string, string> instances, string msgToDisplay)
        {
            Console.WriteLine("select operator or instance to modify partitions:");
            int i = 0;
            foreach (var kv in instances)
            {
                Console.WriteLine($"{++i}={kv.Value}");
            }
            Console.WriteLine("Q or q=Quit");
        }
    }
}

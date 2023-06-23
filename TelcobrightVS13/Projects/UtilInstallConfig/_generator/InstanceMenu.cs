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
    public class Menu
    {
        public static string getSingleChoice(List<string> menuItems, string msgToDisplay)
        {
            List<string> selectedItems = getChoices(menuItems, msgToDisplay);
            return selectedItems.First();
        }
        public static List<string> getChoices(IEnumerable<string> menuItems, string msgToDisplay)
        {
            var menuItemsAsList = menuItems as IList<string> ?? menuItems.ToList();
            Dictionary<string, string> menuItemsDic = createMenuItemsDictionary(menuItemsAsList);
            Console.Clear();
            printMenu(menuItemsDic, msgToDisplay);
            while (true)
            {
                List<int> userInputs = getUserInput();
                if (userInputs[0] ==999999 ) // select all
                {
                    return menuItemsAsList.ToList();
                }
                if (userInputs[0] == 0) // quit case
                {
                    return new List<string>();
                }
                else if (userInputs[0] < 0 || userInputs.Any(i => i > menuItemsDic.Count))// invalid case
                {
                    Console.Clear();
                    Console.WriteLine("Invalid input, try again...");
                    printMenu(menuItemsDic, msgToDisplay);
                }
                else // valid case
                {
                    List<string> selectedNumbers = userInputs.Select(i => i.ToString()).ToList();
                    List<string> selectedItems = menuItemsDic.Where(kv => selectedNumbers.Contains(kv.Key))
                        .Select(kv => kv.Value).ToList();
                    Console.WriteLine($"Selected items: [{string.Join(",", selectedItems)}]");
                    return selectedItems;
                }
            }
        }

        private static Dictionary<string, string> createMenuItemsDictionary(IEnumerable<string> menuItems)
        {
            return menuItems.OrderBy(s=>s).Select((item, index) =>
                         new
                         {
                             item = item,
                             index = index+1//1 based
                         }).ToDictionary(a => a.index.ToString(), a => a.item);
        }

        static List<int> getUserInput()
        {
            string userInput = Console.ReadLine().Trim();
            List<int> finalInputs= new List<int>();
            if (userInput == "a" || userInput == "A")//all
            {
                finalInputs.Add(999999);
                return finalInputs;//999999=all selected
            }
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
            int i = 0;
            foreach (var kv in instances)
            {
                Console.WriteLine($"{++i}={kv.Value}");
            }
            Console.WriteLine("Q or q=Quit");
        }
    }
}

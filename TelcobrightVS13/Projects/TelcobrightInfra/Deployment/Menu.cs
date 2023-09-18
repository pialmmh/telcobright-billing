using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra
{
    public class Menu
    {
        public List<string> MenuItems { get; }
        public string MessageToDisplay { get; }
        public string AllChoicesIndicator { get; }
        public Menu(IEnumerable<string> menuItems, string messageToDisplay, string allChoicesIndicator)
        {
            this.MenuItems = menuItems.ToList();
            this.MessageToDisplay = messageToDisplay;
            this.AllChoicesIndicator = allChoicesIndicator.ToLower();
        }

        public string getSingleChoice(out bool quit)
        {
            quit = false;
            List<string> selectedItems = getChoices();//empty list = quit case
            if (selectedItems.Any() == false)
            {
                quit = true;
                return "";
            }
            return selectedItems.First();
        }
        public string getSingleChoice()
        {
            List<string> selectedItems = getChoices();//empty list = quit case
            return selectedItems.First();
        }
        public List<string> getChoices()
        {
            var menuItemsAsList = this.MenuItems as IList<string> ?? this.MenuItems.ToList();
            Dictionary<string, string> menuItemsDic = createMenuItemsDictionary(menuItemsAsList);
            Console.Clear();
            printMenu(menuItemsDic, this.MessageToDisplay);
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
                    printMenu(menuItemsDic, this.MessageToDisplay);
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

        private Dictionary<string, string> createMenuItemsDictionary(IEnumerable<string> menuItems)
        {
            return menuItems.OrderBy(s=>s).Select((item, index) =>
                         new
                         {
                             item = item,
                             index = index+1//1 based
                         }).ToDictionary(a => a.index.ToString(), a => a.item);
        }

        List<int> getUserInput()
        {
            string userInput = Console.ReadLine().Trim();
            List<int> finalInputs= new List<int>();
            if (!string.IsNullOrEmpty(this.AllChoicesIndicator) 
                && userInput.ToLower() == this.AllChoicesIndicator)//all
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

        void printMenu(Dictionary<string, string> choices, string msgToDisplay)
        {
            Console.WriteLine(msgToDisplay);
            int i = 0;
            foreach (var kv in choices)
            {
                Console.WriteLine($"{++i}={kv.Value}");
            }
            if (!string.IsNullOrEmpty(this.AllChoicesIndicator))
            {
                Console.WriteLine($"[{this.AllChoicesIndicator}=All], [Q/q=Quit]");
            }
            else
            {
                Console.WriteLine($"Q/q=Quit");
            }
        }
    }
}

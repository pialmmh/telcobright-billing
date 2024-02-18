using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions.ConfigHelper
{
    public class ConsoleUtil
    {
        public List<char> _confirmationChars { get; set; }
        public ConsoleUtil(List<char> confirmationChars= null)
        {
            this._confirmationChars = confirmationChars ?? new List<char> {'y', 'Y'};
        }
        public bool getConfirmationFromUser(string promptOrMsg, List<char> confirmationChars=null)
        {
            List<char> confirm= confirmationChars ?? this._confirmationChars;
            Console.WriteLine();
            Console.WriteLine(promptOrMsg);
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();
            return confirm.Contains(keyInfo.KeyChar);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Process
{
    public class PrepaidService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            switch (e.Data)
            {
                case "checkbalance":
                    var msg = "balance checking ...";
                    Send(msg);
                    break;
                case "getbalance":
                    msg = "getting balance ...";
                    Send(msg);
                    break;
                default:
                    msg = "Please send a valid event";
                    Send(msg);
                    break;
            }
            Console.WriteLine(e.Data);
        }
    }
}

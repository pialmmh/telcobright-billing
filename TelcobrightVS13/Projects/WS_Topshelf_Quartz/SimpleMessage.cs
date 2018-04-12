using System;

namespace WS_Telcobright_Topshelf
{
    public interface ISimpleMessage
    {
        String Message { get; }
    }

    public class SimpleMessage : ISimpleMessage
    {

        private String _message;

        public SimpleMessage()
        {
        }

        public SimpleMessage(String message)
        {
            this._message = message;
        }

        public String Message
        {
            get
            {
                return this._message;
            }
            set
            {
                this._message = value;
            }
        }
    }
}

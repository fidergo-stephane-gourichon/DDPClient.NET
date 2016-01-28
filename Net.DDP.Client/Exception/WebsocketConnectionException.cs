using System;

namespace Net.DDP.Client
{
    public class WebsocketConnectionException : Exception
    {
        public WebsocketConnectionException (string message) : base (message)
        {
        }
    }
}


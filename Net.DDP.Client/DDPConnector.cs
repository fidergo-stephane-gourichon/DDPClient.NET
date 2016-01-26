using System;
using WebSocket4Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;

namespace Net.DDP.Client
{
    internal class DDPConnector
    {
        private WebSocket _socket;
        private string _url = string.Empty;
        private int _isWait;

        public IObservable<DDPMessage> Connect (string url)
        {
            _url = url;
            _socket = new WebSocket (_url);
            IObservable<DDPMessage> socketStream = Observable.FromEventPattern<MessageReceivedEventArgs> (
                                                       handler => _socket.MessageReceived += handler, 
                                                       handler => _socket.MessageReceived -= handler)
                .Select (pattern => DeserializeOrReturnErrorObject (pattern.EventArgs.Message));
            _socket.Opened += _socket_Opened;
            _socket.Open ();
            _isWait = 1;

            return socketStream;
        }

        public void Close ()
        {
            _socket.Close ();
        }

        public void Send (string message)
        {
            Debug.WriteLine ("Sending message: " + message + " on thread:  " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            _socket.Send (message);
        }

        void _socket_Opened (object sender, EventArgs e)
        {
            this.Send ("{\"msg\":\"connect\",\"version\":\"pre1\",\"support\":[\"pre1\"]}");
            _isWait = 0;
        }

        private void Wait ()
        {
            while (_isWait != 0 && _socket.State == WebSocketState.Connecting) {
                System.Threading.Thread.Sleep (100);
            }
        }

        private DDPMessage DeserializeOrReturnErrorObject (string jsonString)
        {
            try {
                Debug.WriteLine ("Receiving message: " + jsonString + " on thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
                return JsonConvert.DeserializeObject<DDPMessage> (jsonString);
            } catch (Exception e) {
                Debug.WriteLine (e);
                // Sacrificing thoroughness for the sake of simplicity here. Good tradeoff in this specific case.
                var ddpMessage = new DDPMessage ();
                ddpMessage.Type = DDPType.Error;
                ddpMessage.Error = new Error (null, null, null, DDPErrorType.ClientError);
                return ddpMessage;
            }
        }

    }
}

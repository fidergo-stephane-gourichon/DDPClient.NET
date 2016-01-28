using System;
using WebSocket4Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;
using System.Reactive.Subjects;
using SuperSocket.ClientEngine;

namespace Net.DDP.Client
{
    internal class DDPConnector
    {
        private WebSocket _socket;
        private IConnectableObservable<DDPMessage> _messageStream;
        private Subject<DDPMessage> _errorStream;
        private IObservable<DDPMessage> _mergedStream;

        private string _url = string.Empty;

        public IObservable<DDPMessage> Connect (string url)
        {
            if (string.IsNullOrWhiteSpace (url)) {
                throw new ArgumentNullException ("url");
            }

            _url = url;
            _socket = new WebSocket (_url);

            _messageStream = Observable.FromEventPattern<MessageReceivedEventArgs> (
                handler => _socket.MessageReceived += handler, 
                handler => _socket.MessageReceived -= handler)
				.Do (m => Debug.WriteLine ("T {0} - DDPClient - Incoming: {1}",
                System.Threading.Thread.CurrentThread.ManagedThreadId, 
                m.EventArgs.Message))
                .Select (rawMessage => DeserializeOrReturnErrorObject (rawMessage.EventArgs.Message))
                .Publish ();

            _errorStream = new Subject<DDPMessage> ();
            _mergedStream = Observable.Merge (new IObservable<DDPMessage> [] {
                _errorStream.AsObservable (),
                _messageStream
            });

            _socket.Opened += _socket_Opened;
            _socket.Error += _socket_Error;
            _socket.Closed += _socket_Closed;

            _messageStream.Connect ();

            _socket.Open ();

            return _mergedStream;
        }

        public void Close ()
        {
            _socket.Close ();
        }

        public void Send (string message)
        {
            Debug.WriteLine ("T {0} - DDPClient - Outgoing: {1} ", System.Threading.Thread.CurrentThread.ManagedThreadId, message);
            _socket.Send (message);
        }

        void _socket_Opened (object sender, EventArgs e)
        {
            this.Send ("{\"msg\":\"connect\",\"version\":\"pre1\",\"support\":[\"pre1\"]}");
        }

        void _socket_Closed (object sender, EventArgs e)
        {
            Debug.WriteLine ("Socket closed: " + e + " on " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            _errorStream.OnError (new WebsocketConnectionException ("Websocket was closed"));
        }

        void _socket_Error (object sender, ErrorEventArgs e)
        {
            Debug.WriteLine ("Socket error: " + e.Exception + " on " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            // Sacrificing thoroughness for the sake of simplicity here. Good tradeoff in this specific case.
            var ddpMessage = new DDPMessage ();
            ddpMessage.Type = DDPType.Error;
            ddpMessage.Error = new Error (null, null, null, DDPErrorType.ClientError);
            _errorStream.OnNext (ddpMessage);
        }

        private DDPMessage DeserializeOrReturnErrorObject (string jsonString)
        {
            try {
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

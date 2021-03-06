﻿using System;
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
        private const string ConnectDDPMessage = "{\"msg\":\"connect\",\"version\":\"1\",\"support\":[\"1\",\"pre2\",\"pre1\"]}";
        //"{\"msg\":\"method\",\"method\":\"login\",\"params\":[{\"resume\":\"-FucQvb_BdhlwINJHXi09yxLvmNORiY8JVP8bj5QrH-\"}],\"id\":\"1\"}"
        private object aLock = new object ();

        private WebSocket _socket;
        private IObservable<DDPMessage> _messageStream;
        private IObservable<DDPMessage> _errorStream;
        private IObservable<DDPMessage> _closedStream;
        private IConnectableObservable<DDPMessage> _mergedStream;

        private string _url = string.Empty;

        //public string Url => _url;

        public IObservable<DDPMessage> Connect (string url)
        {
            Validations.ValidateUrl (url);

            lock (aLock) {

                Close ();

                _url = url;
                _socket = new WebSocket (_url);

                _messageStream = Observable.FromEventPattern<MessageReceivedEventArgs> (
                    handler => _socket.MessageReceived += handler, 
                    handler => _socket.MessageReceived -= handler)
                    .Select(epmrea => epmrea.EventArgs.Message)
                    .Do(m => Debug.WriteLine("T {0} - DDPClient - Incoming: {1}", Thread.CurrentThread.ManagedThreadId,
                   m))
                   .Where(m => m != "{\"server_id\":\"0\"}") // https://forums.meteor.com/t/what-is-server-id-in-ddp/2061
                   .Select(m =>
                   {
                       if (m == "{\"msg\":\"ping\"}")
                       {
                           Debug.WriteLine("Received ddp-level ping, replying with ddp-level pong.");
                           this.Send("{\"msg\":\"pong\"}");
                           return null;
                       }
                       return DeserializeOrReturnErrorObject(m);
                   })
                   .Where(m => m != null); // Don't notify for already handled messages like ddp-level ping.

                _errorStream = Observable.FromEventPattern<ErrorEventArgs> (
                    handler => _socket.Error += handler, 
                    handler => _socket.Error -= handler)
                .Do (m => Debug.WriteLine ("T {0} - DDPClient - Error: {1}", Thread.CurrentThread.ManagedThreadId, 
                    m.EventArgs.Exception))
                .SelectMany(
                    m => Observable.Throw<DDPMessage>(m.EventArgs.Exception)
                    );

                _closedStream = Observable.FromEventPattern (
                    handler => _socket.Closed += handler, 
                    handler => _socket.Closed -= handler)
                .Do (m => Debug.WriteLine ("T {0} - DDPClient - Socket closed", Thread.CurrentThread.ManagedThreadId))
                .SelectMany (m => Observable.Throw <DDPMessage> (new WebsocketConnectionException ("Websocket was closed")));

                _mergedStream = Observable.Merge (new IObservable<DDPMessage> [] {
                    _messageStream, _errorStream, _closedStream
                }).Publish ();

                _socket.Opened += _socket_Opened;

                _mergedStream.Connect ();

                _socket.Open ();

                return _mergedStream;
            }
        }

        public void Send (string message)
        {
            if (message == null) {
                throw new ArgumentNullException ("message");
            }
            if (_socket == null || !WebSocketState.Open.Equals (_socket.State)) {
                throw new InvalidOperationException ("The websocket connections is not opened and/or valid");
            }

            Debug.WriteLine ("T {0} - DDPClient - Outgoing: {1} ", System.Threading.Thread.CurrentThread.ManagedThreadId, message);
            _socket.Send (message);
        }

        private void _socket_Opened (object sender, EventArgs e)
        {
            this.Send (ConnectDDPMessage);
        }

        private DDPMessage DeserializeOrReturnErrorObject (string jsonString)
        {
            try {
                return JsonConvert.DeserializeObject<DDPMessage> (jsonString);
            } catch (Exception e) {
                Debug.WriteLine (e);
                // Sacrificing thoroughness for the sake of simplicity here. Good tradeoff in this specific case.
                return BuildErrorDDPMessage ();
            }
        }

        private DDPMessage BuildErrorDDPMessage ()
        {
            var ddpMessage = new DDPMessage ();
            ddpMessage.Type = DDPType.Error;
            ddpMessage.Error = new Error (null, null, null, DDPErrorType.ClientError);
            return ddpMessage;
        }

        public void Close ()
        {
            lock (aLock) {
                if (_socket != null) {
                    var wsState = _socket.State;
                    if (!WebSocketState.Closing.Equals (wsState)
                        && !WebSocketState.Closed.Equals (wsState)
                        && !WebSocketState.None.Equals (wsState)) {
                        Debug.WriteLine ("Closing socket " + _socket.GetHashCode ());
                        _socket.Close ();    
                    } 
                    _socket = null;
                }
            }
        }
    }
}

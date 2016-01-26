using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace Net.DDP.Client.Test
{
    static class Program
    {
        static void Main (string[] args)
        {
            Debug.WriteLine ("Starting test run... on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            DDPClient client = new DDPClient ();

            client.Connect ("ws://192.168.99.100:3000/websocket").Do (m => Debug.WriteLine (m)).Where (
                ddpMessage => DDPType.Connected.Equals (ddpMessage.Type)
            ).Subscribe <DDPMessage> (m => Debug.WriteLine ("Received: " + m));



//            client.Connect ("ws://192.168.99.100:3000/websocket").Subscribe<DDPMessage> (
//                msg => {
//                    Debug.WriteLine ("OK: " + msg);
//                    Debug.WriteLine ("Observing on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
//                },
//                e => Debug.WriteLine (e)
//            );

            while (!"bye".Equals (System.Console.ReadLine ())) {
                
            }
        }
    }
}

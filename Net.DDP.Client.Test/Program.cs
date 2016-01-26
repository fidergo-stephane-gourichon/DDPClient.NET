using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.IO;

namespace Net.DDP.Client.Test
{
    class Program
    {
        static void Main (string[] args)
        {

            Program program = new Program ();
            program.Play ();
        }

        public void Play ()
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

            startUILoop ();
        }

        public void startUILoop ()
        {
            Task<string> inputTask = System.Console.In.ReadLineAsync ();
            while (true) {
                if (inputTask.IsCompleted) {
                    string input = inputTask.Result;

                    if ("bye".Equals (input)) {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace (input)) {
                        //                        chatClient.SendTextMessage (input, "GENERAL");
                    }

                    inputTask = System.Console.In.ReadLineAsync ();
                }
                // Throttle the thread a little bit

                System.Threading.Thread.Sleep (10);
            }
            System.Console.WriteLine ("See you later!");
        }
    }
}

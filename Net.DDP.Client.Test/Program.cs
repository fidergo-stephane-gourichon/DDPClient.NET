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
        DDPClient client;

        static void Main (string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Program program = new Program ();
            program.Play ();
        }

        public void Play ()
        {
            Debug.WriteLine ("Starting test run... on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            client = new DDPClient ();

            client.Connect ("ws://192.168.99.100:3000/websocket")
                .Subscribe <DDPMessage> (
                m => Debug.WriteLine ("Received: " + m),
                e => Debug.WriteLine ("Exception: " + e),
                () => Debug.WriteLine ("OnCompleted")
            );



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

                        if ("connect".Equals (input)) {
                            client.Connect ("ws://192.168.99.100:3000/websocket")
                                    .Subscribe <DDPMessage> (
                                m => Debug.WriteLine ("Received on console: " + m),
                                e => Debug.WriteLine ("Exception on console: " + e),
                                () => Debug.WriteLine ("OnCompleted on console")
                            );
                        }
                        client.Call (input).Subscribe <DDPMessage> (
                            m => Debug.WriteLine ("Received on input: " + m),
                            e => Debug.WriteLine ("Exception on input: " + e),
                            () => Debug.WriteLine ("OnCompleted on input")
                        );
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

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

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Program program = new Program();
            program.Play();
        }

        public void Play()
        {
            Debug.WriteLine("Starting test run... on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            client = new DDPClient();

            client.Connect("ws://192.168.56.1:3000/websocket")
                .Subscribe<DDPMessage>(OnDdpMessageReceived, OnDdpException, OnDdpCompleted);



            //            client.Connect ("ws://192.168.99.100:3000/websocket").Subscribe<DDPMessage> (
            //                msg => {
            //                    Debug.WriteLine ("OK: " + msg);
            //                    Debug.WriteLine ("Observing on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            //                },
            //                e => Debug.WriteLine (e)
            //            );

            startUILoop();
        }

        void OnDdpMessageReceived(DDPMessage ddpMsg)
        {
            Debug.WriteLine("Received: " + ddpMsg.DDPMessageData);
        }

        void OnDdpException(Exception ex)
        {
            Debug.WriteLine("Exception: " + ex);
        }

        void OnDdpCompleted()
        {
            Debug.WriteLine("OnCompleted");
        }

        public void startUILoop()
        {
            Task<string> inputTask = System.Console.In.ReadLineAsync();
            while (true)
            {
                if (inputTask.IsCompleted)
                {
                    string input = inputTask.Result;
                    try
                    {
                        if ("bye".Equals(input))
                        {
                            break;
                        }

                        else if ("l".Equals(input))
                        {
                            client.Call("login", new { user = new { email = "a@b.c" }, password = "b" })
                                        .Subscribe<DDPMessage>(
                                    m => Debug.WriteLine("Received on login: " + m),
                                    e => Debug.WriteLine("Exception on login: " + e),
                                    () => Debug.WriteLine("OnCompleted on login")
                                );
                        }

                        else if ("lwp".Equals(input))
                        {
                            client.Call("login", new { user = new { email = "a@b.c" }, password = "wrongpassword" })
                                        .Subscribe<DDPMessage>(
                                    m => Debug.WriteLine("Received on wrongpassword: " + m),
                                    e => Debug.WriteLine("Exception on wrongpassword: " + e),
                                    () => Debug.WriteLine("OnCompleted on wrongpassword")
                                );
                        }

                        else if ("lwe".Equals(input))
                        {
                            client.Call("login", new { user = new { email = "unknown@not.known" }, password = "wrongpassword" })
                                        .Subscribe<DDPMessage>(
                                    m => Debug.WriteLine("Received on unknown user: " + m),
                                    e => Debug.WriteLine("Exception on unknown user: " + e),
                                    () => Debug.WriteLine("OnCompleted on unknown user")
                                );
                        }

                        else if ("s".Equals(input))
                        {
                            client.Subscribe("parties");
                        }

                        else if (!string.IsNullOrWhiteSpace(input))
                        {

                            if ("connect".Equals(input))
                            {
                                client.Connect("ws://192.168.99.100:3000/websocket")
                                        .Subscribe<DDPMessage>(
                                    m => Debug.WriteLine("Received on connect: " + m),
                                    e => Debug.WriteLine("Exception on connect: " + e),
                                    () => Debug.WriteLine("OnCompleted on connect")
                                );
                            }
                            client.Call(input).Subscribe<DDPMessage>(
                                m => Debug.WriteLine("Received on Call: " + m),
                                e => Debug.WriteLine("Exception on Call: " + e),
                                () => Debug.WriteLine("OnCompleted on Call")
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("********************************** Got exception: " + ex);
                    }
                    inputTask = System.Console.In.ReadLineAsync();
                }
                // Throttle the thread a little bit

                System.Threading.Thread.Sleep(10);
            }
            System.Console.WriteLine("See you later!");
        }
    }
}

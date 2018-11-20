using lightdb.sdk;
using System;
using System.IO;
using System.Threading.Tasks;

namespace lightdb.testclient
{
    class Program
    {

        static lightdb.sdk.WebsocketBase client = new sdk.WebsocketBase();

        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += (s, e) =>
              {
                  Console.WriteLine("error on ============>" + e.ToString());
              };
            StartClient();
            Loops();
        }
        static async void Loops()
        {


            Console.WriteLine("Hello World!");
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    Environment.Exit(0);
                    return;

                }
                if (line == "ping")
                {
                    try
                    {
                        var pingms = client.PostPing();
                        Console.WriteLine("ping=" + pingms);
                        //Task.WaitAll(ping());
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("error on ping.");
                    }
                    continue;
                }
                if (line == "db.state")
                {
                    var msg =  client.Post_getdbstate();
                    Console.WriteLine("msg. open=" + msg.dbopen);
                    Console.WriteLine("height=" + msg.height);
                    foreach(var w in msg.writer)
                    {
                        Console.WriteLine("writer=" + w);
                    }
                }
            }
        }

        static async void StartClient()
        {
            client.OnDisconnect += async () =>
            {
                Console.WriteLine("OnDisConnect.");
            };
            //client.OnRecv_Unknown += async (msg) =>
            //  {
            //      Console.WriteLine("got unknown msg:" + msg.Cmd);
            //  };
            await client.Connect(new Uri("ws://127.0.0.1:80/ws"));
            Console.WriteLine("connected.");

            for (var i = 0; i < 100; i++)
            {
                var pingms = client.PostPing();
                Console.WriteLine("ping=" + pingms);
            }
        }


    }


}

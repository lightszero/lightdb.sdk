using LightDB.SDK;
using LightDB;
using System;
using System.IO;
using System.Threading.Tasks;

namespace lightdb.testclient
{
    class Program
    {

        static LightDB.SDK.WebsocketBase client = new LightDB.SDK.WebsocketBase();
        static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();
        static void AddMenu(string cmd, string desc, Action<string[]> onMenu)
        {
            menuItem[cmd.ToLower()] = onMenu;
            menuDesc[cmd.ToLower()] = desc;
        }
        static void InitMenu()
        {
            AddMenu("exit", "exit application", (words) => { Environment.Exit(0); });
            AddMenu("help", "show help", ShowMenu);
            AddMenu("ping", "ping server.", Ping);
            AddMenu("db.state", "show db state", DBState);
            AddMenu("db.usesnap", "open a db snap.", DBUseSnap);
            AddMenu("db.unusesnap", "close a db snap.", DBUnuseSnap);
            AddMenu("db.snapheight", "check snapheight.", DBsnapheight);
            AddMenu("db.block", "show cur dbblock ,use db.block [n].", DBGetBlock);
            AddMenu("db.blockhash", "show cur dbblock ,use db.blockhash [n].", DBGetBlockHash);
            AddMenu("db.getwriter", "get all writers.", DBGetWriter);

        }
        static void ShowMenu(string[] words = null)
        {
            Console.WriteLine("==Menu==");
            foreach (var key in menuItem.Keys)
            {
                var line = "  " + key + " - ";
                if (menuDesc.ContainsKey(key))
                    line += menuDesc[key];
                Console.WriteLine(line);
            }
        }
        static void MenuLoop()
        {
            while (true)
            {
                try
                {
                    Console.Write("-->");
                    var line = Console.ReadLine();
                    var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        var cmd = words[0].ToLower();
                        if (cmd == "?")
                        {
                            ShowMenu();
                        }
                        else if (menuItem.ContainsKey(cmd))
                        {
                            menuItem[cmd](words);
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("err:" + err.Message);
                }
            }
        }
        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += (s, e) =>
              {
                  Console.WriteLine("error on ============>" + e.ToString());
              };
            InitMenu();
            StartClient();
            MenuLoop();
            //Loops();
        }
        static void Ping(string[] words)
        {
            var pingms = client.PostPing();
            Console.WriteLine("ping=" + pingms);

        }
        static void DBState(string[] words)
        {
            var msg = client.Post_getdbstate();
            Console.WriteLine("msg. open=" + msg.dbopen);
            Console.WriteLine("height=" + msg.height);
            foreach (var w in msg.writer)
            {
                Console.WriteLine("writer=" + w);
            }
        }

        static void DBGetBlock(string[] words)
        {
            UInt64 blockid = UInt64.Parse(words[1]);
            var msg = client.Post_snapshot_getblock(lastSnapheight.Value, blockid);
            //var msg = client.Post_snapshot_getvalue(lastSnapheight.Value, protocol_Helper.systemtable_block, BitConverter.GetBytes(blockid));
            var v = LightDB.DBValue.FromRaw(msg.data);
            var task = LightDB.WriteTask.FromRaw(v.value);
            Console.WriteLine("got info=" + msg.ToString());
            foreach(var i in task.items)
            {
                Console.WriteLine("item=" + i.ToString());
            }
            if (task.extData != null)
            {
                foreach (var e in task.extData)
                {
                    Console.WriteLine("extdata=" + e.Key + " len=" + e.Value.Length);
                }
            }
        }
        static void DBGetBlockHash(string[] words)
        {
            UInt64 blockid = UInt64.Parse(words[1]);
            var msg = client.Post_snapshot_getblockhash(lastSnapheight.Value, blockid);
            //var msg = client.Post_snapshot_getvalue(lastSnapheight.Value, protocol_Helper.systemtable_block, BitConverter.GetBytes(blockid));
            var v = LightDB.DBValue.FromRaw(msg.data);
            Console.WriteLine("got hash=" + v.value.ToString_Hex());
        }
        static void DBGetWriter(string[] words)
        {
            var msg = client.Post_snapshot_getwriter(lastSnapheight.Value);
            Console.WriteLine("got writer count=" + msg.writer.Count);
            foreach(var item in msg.writer)
            {
                Console.WriteLine("writer=" + item);
            }

        }
        static UInt64? lastSnapheight;
        static void DBUseSnap(string[] words)
        {
            var msg = client.Post_usesnapshot();
            Console.WriteLine("snapshot got height=" + msg.snapheight);
            lastSnapheight = msg.snapheight;
        }
        static void DBUnuseSnap(string[] words)
        {
            var msg = client.Post_unusesnapshot(lastSnapheight.Value);
            Console.WriteLine("snapshot free=" + msg.remove);
            lastSnapheight = null;
        }
        static void DBsnapheight(string[] words)
        {
            var msg = client.Post_snapshot_dataheight(lastSnapheight.Value);
            Console.WriteLine("snapshot height=" + lastSnapheight + "," + msg.dataheight);
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

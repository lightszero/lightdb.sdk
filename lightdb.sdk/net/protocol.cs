using System;
using System.Collections.Generic;
using System.Text;

namespace lightdb.sdk
{
    public static class protocol_Helper
    {
        public static NetMessage PostMsg(this WebsocketBase socket, sdk.NetMessage msg)
        {
            NetMessage __msg = null;
            socket.SendWithOnceCallback(msg, async (msgback) =>
            {
                __msg = msgback;
            });
            while (__msg == null)
            {
                System.Threading.Thread.Sleep(1);
            }
            return __msg;
        }
        public static byte[] ToBytes_UTF8Encode(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }
        public static string ToString_UTF8Decode(this byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
    public static class protocol_Ping
    {

        public static NetMessage CreateSendMsg()
        {
            var msg = sdk.NetMessage.Create("_ping");
            return msg;
        }

        public static bool PraseRecvMsg(NetMessage msg)
        {
            if (msg.Cmd == "_ping.back")
                return true;
            else
                throw new Exception("error message.");
        }

        public static int PostPing(this WebsocketBase socket)
        {
            DateTime t0 = DateTime.Now;

            var msg = protocol_Ping.CreateSendMsg();
            var msgrecv = socket.PostMsg(msg);
            var s = protocol_Ping.PraseRecvMsg(msgrecv);

            DateTime t1 = DateTime.Now;
            return (int)((t1 - t0).TotalMilliseconds);
        }
    }

    public static class protocol_getdbstate
    {
        public static NetMessage CreateSendMsg()
        {
            var msg = sdk.NetMessage.Create("_db.state");
            return msg;
        }
        public class message
        {
            public bool dbopen;
            public UInt64 height;
            public List<string> writer = new List<string>();
        }
        public static message PraseRecvMsg(NetMessage msg)
        {
            if (msg.Cmd == "_db.state.back")
            {
                message data = new message();
                if (msg.Params["dbopen"][0] == 1)
                    data.dbopen = true;
                if(data.dbopen)
                {
                    data.height = BitConverter.ToUInt64(msg.Params["height"], 0);
                }
                foreach(var key in msg.Params.Keys)
                {
                    if(key.IndexOf("writer")==0)
                    {
                        data.writer.Add(msg.Params[key].ToString_UTF8Decode());
                    }
                }
                return data;
            }
            else
                throw new Exception("error message.");
        }
        public static message Post_getdbstate(this WebsocketBase socket)
        {
            var msg = protocol_getdbstate.CreateSendMsg();
            var msgrecv = socket.PostMsg(msg);
            var s = protocol_getdbstate.PraseRecvMsg(msgrecv);
            return s;
        }
    }
}

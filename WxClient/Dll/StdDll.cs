using System;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace WxClient.dll
{
    public static class StdDll
    {
        private static Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        delegate void PConnected(int clientId);

        delegate void PRecvMessage(int clientId, string jsonData, int dataSize);

        delegate void PQuit(int clientId);

        static void Connect(int clientId)
        {
            Logger.Info($"新的客户端连接: {clientId}");
        }

        static void Recv(int clientId, string jsonData, int dataSize)
        {
            dynamic data = JsonConvert.DeserializeObject(jsonData) ?? "";
            foreach (var obj in data)
            {
                if (obj.Name == "type")
                {
                    var val = obj.Value;
                    if (val == 20500) // 心跳数据，忽略
                    {
                        return;
                    }
                }
            }

            Console.WriteLine("\n接收到 {0} 的消息:\n", clientId);
            Console.WriteLine(jsonData);
        }

        static void Quit(int clientId)
        {
            Console.WriteLine("\n客户端  {0} 断开连接", clientId);
        }

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdInitSocket", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdInitSocket(PConnected conn, PRecvMessage recv, PQuit quit);

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdInitDllPath", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdInitDllPath(String path);

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdRun", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdRun();

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdGetLocalWechatVersion", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdGetLocalWechatVersion(char[] version, int versionMaxLength);

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdOpenWechat", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdOpenWechat();

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdSend", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        static extern int WXCmdSend(int clientID, byte[] jsonData);

        [DllImport("WXCommand.dll", EntryPoint = "WXCmdStop", ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        static extern int WXCmdStop();

        // 发送json
        static void SendJson(int clientID, object json)
        {
            // 将json序列化为字符串
            var str = JsonConvert.SerializeObject(json);
            // 将json字符串转换成 utf8 bytes
            var jsonBytes = Encoding.UTF8.GetBytes(str);
            WXCmdSend(clientID, jsonBytes);
        }

        public static void Init()
        {
            var connect = new PConnected(Connect);
            var recv = new PRecvMessage(Recv);
            var quit = new PQuit(Quit);
            // 1 初始化网络连接
            WXCmdInitSocket(connect, recv, quit);

            // 2 初始化注入目录
            WXCmdInitDllPath("C:\\Dll\\vx_3.6.0.18_release.dll");

            // 3 (可选) 获取微信版本号
            /*Char[] version = new Char[100];
            WXCmdGetLocalWechatVersion(version, 100);
            Logger.Info($"微信版本:{new string(version)}");*/

            // 4 运行
            WXCmdRun();
        }
    }
}
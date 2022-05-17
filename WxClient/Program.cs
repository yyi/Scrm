using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;
using WxClient.Channel;
using WxClient.dll;

namespace WxClient
{
    internal static class Program
    {
        private static Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("启动应用");
            StdDll.Init();
            WxSocketClient.Instance.Start("127.0.0.1", 12345);
        }
    }
}
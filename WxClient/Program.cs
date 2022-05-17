using NLog;
using WxClient.Channel;
using WxClient.dll;

namespace WxClient
{
    internal static class Program
    {
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("启动应用");
            StdDll.Init();
            WxSocketClient.Instance.Start("localhost", 12345);
        }
    }
}
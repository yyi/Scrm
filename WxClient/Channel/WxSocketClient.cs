using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace WxClient.Channel
{
    public class WxSocketClient
    {
        private Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        public static readonly WxSocketClient Instance = new WxSocketClient();

        private IPEndPoint _endPoint;

        private volatile bool _connected = false;

        private volatile bool _run = true;

        private volatile NetworkStream _stream;


        public void Start(string ip, int port)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var thread = new Thread(Run);
            thread.Start();
        }

        public void Stop()
        {
            _run = false;
        }

        private void Run()
        {
            while (_run)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.Connect(_endPoint);
                        if (!client.Connected)
                        {
                            Logger.Info($"客户端连接失败 {_endPoint}");
                            Disconnected();
                            continue;
                        }

                        Connected(client.GetStream());
                        while (_connected)
                        {
                            ReceiveCommand();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "连接异常");
                    Disconnected();
                }
            }
        }

        private void ReceiveCommand()
        {
            var lengthByte = new byte[4];
            SyncReadBuffer(_stream, ref lengthByte);
            var len = BinaryPrimitives.ReadUInt32BigEndian(lengthByte);
            Console.WriteLine("响应长度{0}", len);
            var responseBytes = new byte[len];
            SyncReadBuffer(_stream, ref responseBytes);
            var commandId = responseBytes[0];
            var bodyLen = BinaryPrimitives.ReadInt32BigEndian(responseBytes) & 0x00FFFFFF;
            Console.WriteLine("报文类型{0} 包体长度{1}", commandId, bodyLen);
            var responseMessage = Encoding.UTF8.GetString(responseBytes, 4, bodyLen);
            Console.WriteLine();
            Console.WriteLine("Response Received From Server:");
            Console.WriteLine(responseMessage);
        }

        private static void SyncReadBuffer(Stream stream, ref byte[] buffer)
        {
            var binaryReader = new BinaryReader(stream);
            buffer = binaryReader.ReadBytes(buffer.Length);
        }

        private void Disconnected()
        {
            _connected = false;
            _stream = null;
        }

        private void Connected(NetworkStream stream)
        {
            _stream = stream;
            _connected = true;
        }
    }
}
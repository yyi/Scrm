using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WxClient.dll;

namespace WxClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            StdDll.Init();
            const int port = 12345;
            var address = IPAddress.Parse("127.0.0.1");
            var messages = new string[]
            {
                "Hello server | Return this payload to sender!",
                "To the server | Send this payload back to me!",
                "Server Header | Another returned message.",
                "Header Value | Payload to be returned",
                "TERMINATE"
            };
            var i = 0;
            while (i < messages.Length)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.Connect(address, port);
                        if (client.Connected)
                        {
                            Console.WriteLine("We've connected from the client");
                        }

                        var bytes = Encoding.UTF8.GetBytes(messages[i++]);
                        using (var requestStream = client.GetStream())
                        {
                            //await requestStream.WriteAsync(bytes, 0, bytes.Length);
                            var lengthByte = new byte[4];
                            SyncReadBuffer(requestStream, lengthByte);
                            // await requestStream.ReadAsync(lengthByte, 0, lengthByte.Length);
                            var len = BinaryPrimitives.ReadUInt32BigEndian(lengthByte);
                            //   var len = BitConverter.ToInt32(lengthByte, 0);
                            Console.WriteLine("响应长度{0}", len);
                            var responseBytes = new byte[len];
                            //   await requestStream.ReadAsync(responseBytes, 0, responseBytes.Length);
                            SyncReadBuffer(requestStream, responseBytes);
                            var commandId = responseBytes[0];
                            var bodyLen = BinaryPrimitives.ReadInt32BigEndian(responseBytes) & 0x00FFFFFF;
                            Console.WriteLine("报文类型{0} 包体长度{1}", commandId, bodyLen);
                            var responseMessage = Encoding.UTF8.GetString(responseBytes, 4, bodyLen);
                            Console.WriteLine();
                            Console.WriteLine("Response Received From Server:");
                            Console.WriteLine(responseMessage);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("重新连接{0}", e);
                }

                var sleepDuration = new Random().Next(2000, 10000);
                Console.WriteLine($"Generating a new request in {sleepDuration / 1000} seconds");
                Thread.Sleep(sleepDuration);
            }
        }

        private static void SyncReadBuffer(Stream stream, byte[] buffer)
        {
            var currentBufferIndex = 0;
            var remainLength = buffer.Length;
            while (true)
            {
                var readLength = stream.Read(buffer, currentBufferIndex, remainLength);
                if (readLength == 0) throw new ArgumentException("客户端已经断开");
                remainLength -= readLength;
                if (remainLength > 0)
                    currentBufferIndex += readLength;
                else
                    break;
            }
        }
    }
}
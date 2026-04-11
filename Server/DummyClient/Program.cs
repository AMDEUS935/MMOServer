using DummyClient.Session;
using ServerCore;
using System;
using System.Net;
using System.Threading;

namespace DummyClient
{
    public enum TestMode
    {
        MoveOnly,
        SkillOnly,
        Mixed
    }

    class Program
    {
        public static TestMode Mode { get; } = TestMode.MoveOnly;
        static int DummyClientCount { get; } = 200;

        static void Main(string[] args)
        {
            Console.WriteLine($"[더미클라이언트] 시나리오: {Mode} / {DummyClientCount}명 접속 준비 중...");
            Thread.Sleep(3000);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => SessionManager.Instance.Generate(), DummyClientCount);

            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}

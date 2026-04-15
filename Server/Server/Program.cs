using Server.Data;
using Server.Game;
using Server.Game.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
			while (true)
			{
				GameLogic.Instance.Update();
				Thread.Sleep(0);
			}
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			// GameLogicTask
			{
				Task gameLogicTask = new Task(GameLogicTask);
				gameLogicTask.Start();
			}

			while (true)
			{
				Thread.Sleep(100);
			}
		}
	}
}



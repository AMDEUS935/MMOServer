using DummyClient;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Timers;

public class ServerSession : PacketSession
{
	public int DummyId { get; set; }
	public int PosX { get; set; }
	public int PosY { get; set; }

	static Random _rand = new Random();
	static MoveDir[] _dirs = { MoveDir.Up, MoveDir.Down, MoveDir.Left, MoveDir.Right };

	Timer _moveTimer = new Timer();
	Timer _skillTimer = new Timer();

	public void StartPlay(int posX, int posY)
	{
		PosX = posX;
		PosY = posY;

		// 동시 폭발 방지: 각 클라이언트마다 시작 시점을 0~500ms 랜덤 지연
		int startDelay = _rand.Next(0, 500);

		if (Program.Mode == TestMode.MoveOnly || Program.Mode == TestMode.Mixed)
		{
			_moveTimer.Interval = startDelay + 500;
			_moveTimer.Elapsed += (s, e) =>
			{
				_moveTimer.Interval = 500;
				SendMovePacket();
			};
			_moveTimer.AutoReset = true;
			_moveTimer.Start();
		}

		if (Program.Mode == TestMode.SkillOnly || Program.Mode == TestMode.Mixed)
		{
			_skillTimer.Interval = startDelay + 1000;
			_skillTimer.Elapsed += (s, e) =>
			{
				_skillTimer.Interval = 1000;
				SendSkillPacket();
			};
			_skillTimer.AutoReset = true;
			_skillTimer.Start();
		}
	}

	void SendMovePacket()
	{
		MoveDir dir = _dirs[_rand.Next(0, _dirs.Length)];

		int dx = 0, dy = 0;
		switch (dir)
		{
			case MoveDir.Up:    dy =  1; break;
			case MoveDir.Down:  dy = -1; break;
			case MoveDir.Left:  dx = -1; break;
			case MoveDir.Right: dx =  1; break;
		}

		C_Move movePacket = new C_Move();
		movePacket.PosInfo = new PositionInfo();
		movePacket.PosInfo.State = CreatureState.Moving;
		movePacket.PosInfo.MoveDir = dir;
		movePacket.PosInfo.PosX = PosX + dx;
		movePacket.PosInfo.PosY = PosY + dy;

		Send(movePacket);

		PosX += dx;
		PosY += dy;
	}

	void SendSkillPacket()
	{
		C_Skill skillPacket = new C_Skill();
		skillPacket.Info = new Skill_Info();
		skillPacket.Info.SkillId = 1;

		Send(skillPacket);
	}

	public void Send(IMessage packet)
	{
		string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
		MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
		ushort size = (ushort)packet.CalculateSize();
		byte[] sendBuffer = new byte[size + 4];
		Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
		Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
		Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
		Send(new ArraySegment<byte>(sendBuffer));
	}

	public override void OnConnected(EndPoint endPoint)
	{
		//Console.WriteLine($"OnConnected : {endPoint}");
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		_moveTimer.Stop();
		_skillTimer.Stop();
		//Console.WriteLine($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}
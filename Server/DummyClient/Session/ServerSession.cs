using DummyClient;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class ServerSession : PacketSession
{
	public int DummyId { get; set; }
	public int ObjectId { get; set; }
	public int PosX { get; set; }
	public int PosY { get; set; }

	static Random _rand = new Random();
	static MoveDir[] _dirs = { MoveDir.Up, MoveDir.Down, MoveDir.Left, MoveDir.Right };

	long _nextMoveTick = 0;
	long _nextSkillTick = 0;

	public void StartPlay(int posX, int posY)
	{
		PosX = posX;
		PosY = posY;

		long now = Environment.TickCount64;
		_nextMoveTick = now + _rand.Next(0, 500);
		_nextSkillTick = now + _rand.Next(0, 1000);
	}

	public void Tick()
	{
		long now = Environment.TickCount64;

		if (Program.Mode == TestMode.MoveOnly || Program.Mode == TestMode.Mixed)
		{
			if (now >= _nextMoveTick)
			{
				SendMovePacket();
				_nextMoveTick = now + 500;
			}
		}

		if (Program.Mode == TestMode.SkillOnly || Program.Mode == TestMode.Mixed)
		{
			if (now >= _nextSkillTick)
			{
				SendSkillPacket();
				_nextSkillTick = now + 1000;
			}
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

		int nextX = PosX + dx;
		int nextY = PosY + dy;

		if (!Program.Map.CanGo(nextX, nextY))
			return;

		C_Move movePacket = new C_Move();
		movePacket.PosInfo = new PositionInfo();
		movePacket.PosInfo.State = CreatureState.Moving;
		movePacket.PosInfo.MoveDir = dir;
		movePacket.PosInfo.PosX = nextX;
		movePacket.PosInfo.PosY = nextY;

		Send(movePacket);
	}

	void SendSkillPacket()
	{
		// 서버 State를 Idle로 리셋해야 스킬 조건 통과
		C_Move idlePacket = new C_Move();
		idlePacket.PosInfo = new PositionInfo();
		idlePacket.PosInfo.State = CreatureState.Idle;
		idlePacket.PosInfo.MoveDir = MoveDir.Down;
		idlePacket.PosInfo.PosX = PosX;
		idlePacket.PosInfo.PosY = PosY;
		Send(idlePacket);

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
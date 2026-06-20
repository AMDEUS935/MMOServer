using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
	}
}



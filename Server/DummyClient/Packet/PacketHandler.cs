using DummyClient.Session;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

class PacketHandler
{
    // 세션별 오브젝트 ID 목록. key = DummyId
    static ConcurrentDictionary<int, HashSet<int>> _knownObjects = new ConcurrentDictionary<int, HashSet<int>>();

    // 서버가 입장을 확인하면 패킷 발송을 시작한다
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterPacket = (S_EnterGame)packet;
        ServerSession serverSession = (ServerSession)session;
        _knownObjects.TryAdd(serverSession.DummyId, new HashSet<int>());
        serverSession.StartPlay(enterPacket.Player.PosInfo.PosX, enterPacket.Player.PosInfo.PosY);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet) { }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        ServerSession s = (ServerSession)session;

        if (!_knownObjects.TryGetValue(s.DummyId, out HashSet<int> objs))
            return;

        foreach (ObjectInfo obj in spawnPacket.Objects)
            objs.Add(obj.ObjectId);
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = (S_Despawn)packet;
        ServerSession s = (ServerSession)session;

        if (!_knownObjects.TryGetValue(s.DummyId, out HashSet<int> objs))
            return;

        foreach (int id in despawnPacket.ObjectIds)
            objs.Remove(id);
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet) { }

    public static void S_SkillHandler(PacketSession session, IMessage packet) { }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet) { }

    public static void S_DieHandler(PacketSession session, IMessage packet) { }
}

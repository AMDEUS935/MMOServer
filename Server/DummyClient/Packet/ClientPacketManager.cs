using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

// PacketManager: 서버에서 받은 패킷을 ID로 구분해서 알맞은 핸들러 함수로 연결해준다
class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    // _onRecv:  패킷ID → "바이트 배열을 파싱하는 함수" 매핑
    // _handler: 패킷ID → "파싱된 패킷을 처리하는 함수" 매핑
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv
        = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    Dictionary<ushort, Action<PacketSession, IMessage>> _handler
        = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    // 서버에서 올 수 있는 패킷 종류를 등록한다
    public void Register()
    {
        _onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
        _handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);

        _onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
        _handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);

        _onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
        _handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);

        _onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
        _handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);

        _onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
        _handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);

        _onRecv.Add((ushort)MsgId.SSkill, MakePacket<S_Skill>);
        _handler.Add((ushort)MsgId.SSkill, PacketHandler.S_SkillHandler);

        _onRecv.Add((ushort)MsgId.SChangeHp, MakePacket<S_ChangeHp>);
        _handler.Add((ushort)MsgId.SChangeHp, PacketHandler.S_ChangeHpHandler);

        _onRecv.Add((ushort)MsgId.SDie, MakePacket<S_Die>);
        _handler.Add((ushort)MsgId.SDie, PacketHandler.S_DieHandler);
    }

    // 서버에서 패킷이 도착하면 여기서 ID를 읽고 해당 파싱 함수를 호출한다
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset); count += 2;
        ushort id   = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); count += 2;

        Action<PacketSession, ArraySegment<byte>, ushort> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer, id);
        // 등록되지 않은 ID는 무시 (더미라서 모든 패킷을 처리할 필요가 없다)
    }

    // 바이트 배열 → Protobuf 메시지 객체로 역직렬화 후 핸들러 호출
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
    {
        T pkt = new T();
        pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

        Action<PacketSession, IMessage> action = null;
        if (_handler.TryGetValue(id, out action))
            action.Invoke(session, pkt);
    }
}

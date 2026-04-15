using System;
using System.Collections.Generic;

namespace DummyClient.Session
{
    // SessionManager: 현재 연결된 더미 클라이언트 세션 목록을 관리한다
    public class SessionManager
    {
        // 싱글톤 - 프로그램 전체에서 하나만 존재
        public static SessionManager Instance { get; } = new SessionManager();

        HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
        object _lock = new object();
        int _nextId = 1;

        // 현재 접속 중인 더미 클라이언트 수
        public int Count
        {
            get { lock (_lock) { return _sessions.Count; } }
        }

        // 새 더미 클라이언트 세션을 만들어서 목록에 추가
        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                session.DummyId = _nextId++;
                _sessions.Add(session);
                Console.WriteLine($"Connected ({_sessions.Count}) Players");
                return session;
            }
        }

        // 연결이 끊기면 목록에서 제거
        public void Remove(ServerSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                Console.WriteLine($"Connected ({_sessions.Count}) Players");
            }
        }

        public void Tick()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                    session.Tick();
            }
        }
    }
}

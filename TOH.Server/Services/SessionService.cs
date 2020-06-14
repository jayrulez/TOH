using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TOH.Common.Services;

namespace TOH.Server.Services
{
    public class Session
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public ConcurrentQueue<PingResponse> Stream { get; set; } = new ConcurrentQueue<PingResponse>();
    }

    class SessionService
    {
        private readonly List<Session> _sessions = new List<Session>();

        public void AddSession(Session session)
        {
            _sessions.Add(session);
        }

        public Session GetSessionByToken(string token)
        {
            return _sessions.FirstOrDefault(p => p.Token.Equals(token));
        }
    }
}

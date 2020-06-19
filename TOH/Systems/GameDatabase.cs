using LiteDB;
using System;
using System.Linq;

namespace TOH.Systems
{
    public sealed class GameDatabase
    {
        private class Session
        {
            public string SessionId { get; set; }
        }

        private static readonly Lazy<GameDatabase> lazy = new Lazy<GameDatabase>(() => new GameDatabase(), true);

        public static GameDatabase Instance { get { return lazy.Value; } }

        private LiteDatabase _db;

        private ILiteCollection<Session> Sessions;

        public GameDatabase()
        {
            _db = new LiteDatabase(@"Filename=GameData.db;Password=toh");
            Initialize();
        }

        private void Initialize()
        {
            Sessions = _db.GetCollection<Session>();
            Sessions.EnsureIndex(c => c.SessionId);
        }

        public void SetSessionId(string sessionId)
        {
            Sessions.DeleteAll();

            Sessions.Insert(new Session
            {
                SessionId = sessionId
            });
        }

        public string GetSessionId()
        {
            var session = Sessions.FindAll().ToList();

            if (session.Count == 0)
            {
                return string.Empty;
            }

            return session.First().SessionId;
        }

        public void RemoveSession()
        {
            Sessions.DeleteAll();
        }
    }
}

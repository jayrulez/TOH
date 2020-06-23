using LiteDB;
using System;
using System.Linq;

namespace TOH.Systems
{
    public sealed class GameDatabase
    {
        public class Session
        {
            public string SessionId { get; set; }
            public int PlayerId { get; set; }
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

        public void SetSession(Session session)
        {
            Sessions.DeleteAll();

            Sessions.Insert(session);

            _db.Checkpoint();
        }

        public Session GetSession()
        {
            var session = Sessions.FindAll().ToList();

            if (session.Count == 0)
            {
                return null;
            }

            return session.First();
        }

        public void RemoveSession()
        {
            Sessions.DeleteAll();
        }
    }
}

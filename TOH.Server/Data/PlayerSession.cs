using System;
using TOH.Common.Services;

namespace TOH.Server.Data
{
    public class PlayerSession
    {
        public string Id { get; set; }
        public int PlayerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

        public virtual Player Player { get; set; }
    }

    public static partial class Extensions
    {
        public static PlayerSessionData ToDataModel(this PlayerSession source)
        {
            var destination = new PlayerSessionData
            {
                Id = source.Id,
                PlayerId = source.PlayerId,
                CreatedAt = source.CreatedAt,
                ExpiresAt = source.ExpiresAt
            };

            return destination;
        }
    }
}

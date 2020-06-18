using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace TOH.Common.Services
{
    [DataContract]
    public class PlayerData
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Username { get; set; }

        [DataMember(Order = 3)]
        public int Level { get; set; }
    }

    [DataContract]
    public class PlayerUnitData
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public int PlayerId { get; set; }

        [DataMember(Order = 3)]
        public int UnitId { get; set; }

        [DataMember(Order = 4)]
        public int Level { get; set; }
    }

    [DataContract]
    public class PlayerSessionData
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public int PlayerId { get; set; }

        [DataMember(Order = 3)]
        public DateTime CreatedAt { get; set; }

        [DataMember(Order = 4)]
        public DateTime ExpiresAt { get; set; }
    }

    [ServiceContract]
    public interface IPlayerService
    {
        ServiceResponse<PlayerData> CreatePlayer(string username, CallContext context = default);
        ServiceResponse<PlayerData> GetPlayerById(int id, CallContext context = default);
        ServiceResponse<PlayerData> GetPlayerByUsername(string username, CallContext context = default);
        ServiceResponse<PlayerUnitData> GetPlayerUnitById(int id, CallContext context = default);
        ServiceResponse<List<PlayerUnitData>> GetPlayerUnitsByPlayerId(int playerId, CallContext context = default);
        ServiceResponse<PlayerSessionData> CreatePlayerSession(string username, CallContext context = default);
        ServiceResponse<PlayerSessionData> GetPlayerSessionById(string id, CallContext context = default);
    }
}
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

        [DataMember(Order = 5)]
        public bool IsExpired { get; set; }
    }

    [DataContract]
    public class IdentifierData<T>
    {
        [DataMember(Order = 1)]
        public T Identifier { get; set; }
    }

    [ServiceContract]
    public interface IPlayerService
    {
        ServiceResponse<PlayerData> CreatePlayer(IdentifierData<string> username, CallContext context = default);
        ServiceResponse<PlayerData> GetPlayerById(IdentifierData<int> id, CallContext context = default);
        ServiceResponse<PlayerData> GetPlayerByUsername(IdentifierData<string> username, CallContext context = default);
        ServiceResponse<PlayerUnitData> GetPlayerUnitById(IdentifierData<int> id, CallContext context = default);
        ServiceResponse<List<PlayerUnitData>> GetPlayerUnitsByPlayerId(IdentifierData<int> playerId, CallContext context = default);
        ServiceResponse<PlayerSessionData> CreatePlayerSession(IdentifierData<string> username, CallContext context = default);
        ServiceResponse<PlayerSessionData> GetPlayerSessionById(IdentifierData<string> id, CallContext context = default);
        ServiceResponse<PlayerSessionData> Login(IdentifierData<string> username, CallContext context = default);
    }
}
using Grpc.Core;
using System.Runtime.Serialization;
using System.ServiceModel;
using ProtoBuf.Grpc;
using System.Collections.Generic;

namespace TOH.Common.Services
{
    [DataContract]
    public class PingRequest
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }

    [DataContract]
    public class PingResponse
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }

    [DataContract]
    public class LoginRequest
    {
        [DataMember(Order = 1)]
        public string Username { get; set; }
    }


    [DataContract]
    public class LoginResponse
    {
        [DataMember(Order = 1)]
        public int PlayerId { get; set; }

        [DataMember(Order = 2)]
        public string Token { get; set; }
    }

    [ServiceContract]
    public interface IPlayerService
    {
        OperationResponse<LoginResponse> Login(LoginRequest request, CallContext context = default);
        IAsyncEnumerable<PingResponse> Ping(IAsyncEnumerable<PingRequest> pings, CallContext context = default);
    }
}

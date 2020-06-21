using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using System.Net.Http;
using TOH.Common.Services;

namespace TOH.Network
{
    public class GameServiceClientOptions
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class GameServiceClient
    {
        public IPlayerService PlayerService { get; private set; }

        private GrpcChannel _grpcChannel;

        public GameServiceClient(GameServiceClientOptions options)
        {
            var httpClientHandler = new HttpClientHandler();

            var httpClient = new HttpClient(httpClientHandler);

            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            _grpcChannel = GrpcChannel.ForAddress($"{options.Protocol}://{options.Host}:{options.Port}", new GrpcChannelOptions
            {
                HttpClient = httpClient
            });

            PlayerService = _grpcChannel.CreateGrpcService<IPlayerService>();
        }
    }
}

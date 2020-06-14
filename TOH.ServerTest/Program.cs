using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TOH.Common.Services;

namespace TOH.ServerTest
{
    class Program
    {
        static async IAsyncEnumerable<PingRequest> GetMessages()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new PingRequest()
                {
                    Message = $"Message {i + 1}"
                };

                await Task.Delay(1000);
            }
        }

        static async Task Main(string[] args)
        {
            var httpClientHandler = new HttpClientHandler();

            var httpClient = new HttpClient(httpClientHandler);

            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            var client = GrpcChannel.ForAddress("https://localhost:6501", new GrpcChannelOptions
            {
                HttpClient = httpClient
            });

            var context = new CallContext(new CallOptions(headers: new Metadata()));

            var playerService = client.CreateGrpcService<IPlayerService>();

            var loginResponse = playerService.Login(new LoginRequest
            {
                Username = "Bob"
            }, context);

            context.RequestHeaders.Add("token", loginResponse.Data.Token);

            await foreach (var pingResponse in playerService.Ping(GetMessages(), context))
            {
                Console.WriteLine($"Reply: {pingResponse.Message}");
            }

            Console.ReadKey();
        }
    }
}

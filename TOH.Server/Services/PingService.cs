using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Common.Services;

namespace TOH.Server.Services
{
    class PingService
    {
        private readonly ILogger _logger;
        private readonly SessionService _sessionService;
        private readonly List<Session> _sessions = new List<Session>();

        private ConcurrentQueue<PingResponse> _responses = new ConcurrentQueue<PingResponse>();

        public PingService(SessionService sessionService, ILogger<PingService> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public void ConnectSession(Session session)
        {
            _sessions.Add(session);
        }

        public void DisconnectSession(Session session)
        {
            if (_sessions.Contains(session))
            {
                _sessions.Remove(session);
            }
        }

        public async IAsyncEnumerable<PingResponse> OutputStream(CallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (_responses.TryDequeue(out var response))
                {
                    var pingSession = _sessions.FirstOrDefault(s => s.Token.Equals(context.RequestHeaders.GetString("token")));

                    if (pingSession != null)
                        yield return response;
                    else
                        await Task.Yield();
                }
                else
                {
                    await Task.Yield();
                }
            }

            yield break;
        }

        public async ValueTask HandlePingRequest(IAsyncEnumerable<PingRequest> pings, CallContext context)
        {
            try
            {
                var stream = pings.GetAsyncEnumerator();

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    if (await stream.MoveNextAsync())
                    {
                        _responses.Enqueue(new PingResponse
                        {
                            Message = $"Response: {stream.Current.Message} - {Guid.NewGuid()}"
                        });
                    }
                }

                //var session = _sessionService.GetSessionByToken(context.RequestHeaders.GetString("token"));

                //await foreach (var ping in pings)
                //{
                //    if (context.CancellationToken.IsCancellationRequested)
                //        break;

                //    _logger.LogInformation($"Handling PingRequest: '{ping.Message}'.");

                //    if (session != null)
                //    {
                //        _responses.Enqueue(new PingResponse
                //        {
                //            Message = $"Response: {ping.Message} - {Guid.NewGuid()}"
                //        });
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}

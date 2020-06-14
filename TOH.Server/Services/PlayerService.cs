using Dasync.Collections;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using TOH.Common.Services;

namespace TOH.Server.Services
{

    class PlayerService : IPlayerService
    {
        private readonly PingService _pingService;
        private readonly SessionService _sessionService;
        private readonly ILogger _logger;

        public PlayerService(PingService pingService, SessionService sessionService, ILogger<PlayerService> logger)
        {
            _pingService = pingService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public IAsyncEnumerable<PingResponse> Ping(IAsyncEnumerable<PingRequest> pings, CallContext context)
        {
            var token = context.RequestHeaders.GetString("token");

            var session = _sessionService.GetSessionByToken(token);

            if (session != null)
            {
                try
                {
                    _pingService.ConnectSession(session);
                    return context.FullDuplexAsync(_pingService.OutputStream, pings, _pingService.HandlePingRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);

                    _pingService.DisconnectSession(session);
                }
            }

            return AsyncEnumerable.Empty<PingResponse>();
        }

        public OperationResponse<LoginResponse> Login(LoginRequest request, CallContext context)
        {
            var response = new OperationResponse<LoginResponse>();

            var connectedPlayer = new Session
            {
                Id = 1,
                Token = Guid.NewGuid().ToString()
            };

            _sessionService.AddSession(connectedPlayer);

            return response.Succeed(new LoginResponse
            {
                PlayerId = connectedPlayer.Id,
                Token = connectedPlayer.Token
            });
        }
    }
}

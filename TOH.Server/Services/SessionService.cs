using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Services
{
    public class Session
    {
        public string SessionId { get; set; }
        public int PlayerId { get; set; }
        public IConnection Connection { get; set; }
    }

    public class SessionService
    {
        private ConcurrentDictionary<string, Session> ActiveSessions { get; } = new ConcurrentDictionary<string, Session>();

        private readonly ILogger _logger;

        private readonly IServiceProvider _serviceProvider;

        public SessionService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<SessionService>>();
        }

        public async Task JoinSession(IConnection connection, JoinSessionPacket packet)
        {
            var playerManager = _serviceProvider.GetRequiredService<PlayerManager>();

            var playerSession = playerManager.GetPlayerSessionById(packet.Token);

            if (playerSession == null || playerSession.IsExpired)
            {
                ActiveSessions.TryRemove(packet.Token, out var _);

                await connection.Send(new JoinSessionFailedPacket()
                {
                    Code = JoinSessionFailCode.InvalidSession
                });
            }
            else
            {
                var newSession = new Session
                {
                    SessionId = packet.Token,
                    PlayerId = playerSession.PlayerId,
                    Connection = connection
                };

                if (ActiveSessions.TryGetValue(packet.Token, out var activeSession))
                {
                    if (!activeSession.Connection.Id.Equals(connection.Id))
                    {


                        if (ActiveSessions.TryUpdate(packet.Token, newSession, activeSession))
                        {
                            await connection.Send(new JoinSessionSuccessPacket()
                            {
                                Code = JoinSessionSuccessCode.Updated
                            });

                            // maybe try to send SessionDisconnected?? connection should be closed at this point though so maybe not
                        }
                        else
                        {
                            await connection.Send(new JoinSessionFailedPacket()
                            {
                                Code = JoinSessionFailCode.RetryRequired
                            });
                        }
                    }
                }
                else
                {
                    if (ActiveSessions.TryAdd(packet.Token, newSession))
                    {
                        await connection.Send(new JoinSessionSuccessPacket()
                        {
                            Code = JoinSessionSuccessCode.Added
                        });
                    }
                    else
                    {
                        await connection.Send(new JoinSessionFailedPacket()
                        {
                            Code = JoinSessionFailCode.RetryRequired
                        });
                    }
                }
            }
        }

        public async Task TryDisconnect(IConnection connection)
        {
            var activeSessionEntry = ActiveSessions.FirstOrDefault(a => a.Value.Connection.Id.Equals(connection.Id));

            if (activeSessionEntry.Value != null)
            {
                ActiveSessions.TryRemove(activeSessionEntry.Key, out var activeSession);

                try
                {
                    activeSession.Connection.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Attempt to close session produced: {ex.Message}");
                }
            }

            await Task.Yield();
        }

        public Task<Session> GetActiveSession(IConnection connection)
        {
            var activeSession = ActiveSessions.FirstOrDefault(a => a.Value.Connection.Id.Equals(connection.Id)).Value;

            return Task.FromResult(activeSession);
        }
    }
}

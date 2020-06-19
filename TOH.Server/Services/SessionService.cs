using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Services
{
    public class ActiveSession
    {
        public string SessionId { get; set; }
        public int PlayerId { get; set; }
        public IConnection Connection { get; set; }
    }

    public class SessionService
    {
        private ConcurrentDictionary<string, ActiveSession> ActiveSessions { get; } = new ConcurrentDictionary<string, ActiveSession>();

        private readonly ILogger _logger;

        private readonly PlayerManager _playerManager;

        public SessionService(PlayerManager playerManager, ILogger<SessionService> logger)
        {
            _playerManager = playerManager;
            _logger = logger;
        }

        public async Task JoinSession(IConnection connection, JoinSessionPacket packet)
        {
            var playerSession = _playerManager.GetPlayerSessionById(packet.Token);

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
                if (ActiveSessions.TryGetValue(packet.Token, out var activeSession))
                {
                    if (!activeSession.Connection.Id.Equals(connection.Id))
                    {
                        if (ActiveSessions.TryUpdate(packet.Token, new ActiveSession
                        {
                            SessionId = packet.Token,
                            PlayerId = playerSession.PlayerId,
                            Connection = connection
                        }, activeSession))
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
                    if (ActiveSessions.TryAdd(packet.Token, new ActiveSession
                    {
                        SessionId = packet.Token,
                        PlayerId = playerSession.PlayerId,
                        Connection = connection
                    }))
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

        public Task<ActiveSession> GetActiveSession(IConnection connection)
        {
            var activeSession = ActiveSessions.FirstOrDefault(a => a.Value.Connection.Id.Equals(connection.Id)).Value;

            return Task.FromResult(activeSession);
        }
    }
}

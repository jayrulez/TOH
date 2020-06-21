using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using TOH.Common.Data;
using TOH.Common.Services;
using TOH.Server.Data;

namespace TOH.Server.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly PlayerManager _playerManager;
        private readonly ILogger _logger;
        private readonly RandomService _randomService;

        public PlayerService(PlayerManager playerManager, RandomService randomService, ILogger<PlayerService> logger)
        {
            _playerManager = playerManager;
            _randomService = randomService;
            _logger = logger;
        }

        private bool IsAuthorized(CallContext context)
        {
            if (context.RequestHeaders == null)
                return false;

            var token = context.RequestHeaders.GetString("token");

            if (string.IsNullOrEmpty(token))
                return false;

            var session = _playerManager.GetPlayerSessionById(token);

            return session != null && !session.IsExpired;
        }

        public ServiceResponse<PlayerData> CreatePlayer(IdentifierData<string> username, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.CreatePlayer(username.Identifier);

                // Assign player 5 random units
                // TODO: relocate and clean this up
                var units = DataManager.Instance.Units.ToList();

                if (units.Count >= 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var unit = _randomService.GetRandom(units);

                        _playerManager.CreatePlayerUnit(player.Id, unit.UnitId, 1);
                    }
                }
                // END TODO

                return response.Succeed(player.ToDataModel());
            }
            catch (EntityExistException)
            {
                return response.Fail(ServiceErrors.PlayerExistError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerData> GetPlayerById(IdentifierData<int> id, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.GetPlayerById(id.Identifier);

                if (player == null)
                {
                    return response.Fail(ServiceErrors.PlayerNotFoundError);
                }

                return response.Succeed(player.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerData> GetPlayerByUsername(IdentifierData<string> username, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.GetPlayerByUsername(username.Identifier);

                if (player == null)
                {
                    return response.Fail(ServiceErrors.PlayerNotFoundError);
                }

                return response.Succeed(player.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerUnitData> GetPlayerUnitById(IdentifierData<int> id, CallContext context)
        {
            var response = new ServiceResponse<PlayerUnitData>();

            try
            {
                var playerUnit = _playerManager.GetPlayerUnitById(id.Identifier);

                if (playerUnit == null)
                {
                    return response.Fail(ServiceErrors.PlayerUnitNotFoundError);
                }

                return response.Succeed(playerUnit.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<List<PlayerUnitData>> GetPlayerUnitsByPlayerId(IdentifierData<int> playerId, CallContext context)
        {
            var response = new ServiceResponse<List<PlayerUnitData>>();

            try
            {
                var playerUnits = _playerManager.GetPlayerUnitsByPlayerId(playerId.Identifier);

                return response.Succeed(playerUnits.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerSessionData> CreatePlayerSession(IdentifierData<string> username, CallContext context = default)
        {
            var response = new ServiceResponse<PlayerSessionData>();

            try
            {
                var playerSession = _playerManager.CreatePlayerSession(username.Identifier);

                return response.Succeed(playerSession.ToDataModel());
            }
            catch (EntityNotFoundException)
            {
                return response.Fail(ServiceErrors.PlayerNotFoundError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }
        public ServiceResponse<PlayerSessionData> GetPlayerSessionById(IdentifierData<string> id, CallContext context = default)
        {
            var response = new ServiceResponse<PlayerSessionData>();

            try
            {
                var playerSession = _playerManager.GetPlayerSessionById(id.Identifier);

                if (playerSession == null)
                {
                    return response.Fail(ServiceErrors.PlayerSessionNotFoundError);
                }

                return response.Succeed(playerSession.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerSessionData> Login(IdentifierData<string> username, CallContext context = default)
        {
            var response = new ServiceResponse<PlayerSessionData>();

            try
            {
                var getPlayer = GetPlayerByUsername(username, context);

                if (!getPlayer.IsSuccessful)
                {
                    var createPlayer = CreatePlayer(username, context);

                    if (!createPlayer.IsSuccessful)
                    {
                        return response.Fail(createPlayer.Error);
                    }

                    return CreatePlayerSession(new IdentifierData<string> { Identifier = createPlayer.Data.Username });
                }

                return CreatePlayerSession(new IdentifierData<string> { Identifier = getPlayer.Data.Username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }
    }
}
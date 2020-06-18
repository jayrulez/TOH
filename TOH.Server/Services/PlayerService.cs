﻿using Microsoft.Extensions.Logging;
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
            if(context.RequestHeaders == null)
                return false;

            var token = context.RequestHeaders.GetString("token");

            if (string.IsNullOrEmpty(token))
                return false;

            var session = _playerManager.GetPlayerSessionById(token);

            return session != null && !session.IsExpired;
        }

        public ServiceResponse<PlayerData> CreatePlayer(string username, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.CreatePlayer(username);

                // Assign player 5 random units
                // TODO: relocate and clean this up
                var units = DataManager.Instance.Units.ToList();

                for (int i = 0; i < 5; i++)
                {
                    var unit = _randomService.GetRandom(units);

                    _playerManager.CreatePlayerUnit(player.Id, unit.UnitId, 1);
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

        public ServiceResponse<PlayerData> GetPlayerById(int id, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.GetPlayerById(id);

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

        public ServiceResponse<PlayerData> GetPlayerByUsername(string username, CallContext context)
        {
            var response = new ServiceResponse<PlayerData>();

            try
            {
                var player = _playerManager.GetPlayerByUsername(username);

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

        public ServiceResponse<PlayerUnitData> GetPlayerUnitById(int id, CallContext context)
        {
            var response = new ServiceResponse<PlayerUnitData>();

            try
            {
                var playerUnit = _playerManager.GetPlayerUnitById(id);

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

        public ServiceResponse<List<PlayerUnitData>> GetPlayerUnitsByPlayerId(int playerId, CallContext context)
        {
            var response = new ServiceResponse<List<PlayerUnitData>>();

            try
            {
                var playerUnits = _playerManager.GetPlayerUnitsByPlayerId(playerId);

                return response.Succeed(playerUnits.ToDataModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return response.Fail(ServiceErrors.UnexpectedError);
            }
        }

        public ServiceResponse<PlayerSessionData> CreatePlayerSession(string username, CallContext context = default)
        {
            var response = new ServiceResponse<PlayerSessionData>();

            try
            {
                var playerSession = _playerManager.CreatePlayerSession(username);

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
        public ServiceResponse<PlayerSessionData> GetPlayerSessionById(string id, CallContext context = default)
        {
            var response = new ServiceResponse<PlayerSessionData>();

            try
            {
                var playerSession = _playerManager.GetPlayerSessionById(id);

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
    }
}
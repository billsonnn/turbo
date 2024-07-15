﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Game.Inventory;
using Turbo.Core.Game.Navigator;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.Utilities;
using Turbo.Database.Entities.Players;
using Turbo.Database.Repositories.Player;
using Turbo.Players.Factories;

namespace Turbo.Players
{
    public class PlayerManager(
        ILogger<IPlayerManager> _logger,
        IPlayerFactory _playerFactory,
        INavigatorManager _navigatorManager,
        IServiceScopeFactory _serviceScopeFactory) : Component, IPlayerManager
    {
        private readonly ConcurrentDictionary<int, IPlayer> _players = new();

        protected override async Task OnInit()
        {

        }

        protected override async Task OnDispose()
        {
            await RemoveAllPlayers();
        }

        public IPlayer GetPlayerById(int id)
        {
            if ((id <= 0) || !_players.TryGetValue(id, out IPlayer value)) return null;

            return value;
        }

        public IPlayer GetPlayerByUsername(string username)
        {
            if (username.Length == 0) return null;

            foreach (IPlayer player in _players.Values)
            {
                if ((player == null) || !player.Name.Equals(username)) continue;

                return player;
            }

            return null;
        }

        public async Task<IPlayer> GetOfflinePlayerById(int id)
        {
            var player = GetPlayerById(id);

            if (player != null) return player;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var playerRepository = scope.ServiceProvider.GetService<IPlayerRepository>();

                var playerEntity = await playerRepository.FindAsync(id);

                if (playerEntity == null) return null;

                player = _playerFactory.Create(playerEntity);
            }

            catch (Exception ex)
            {

            }

            return player;
        }

        public async Task<IPlayer> GetOfflinePlayerByUsername(string username)
        {
            var player = GetPlayerByUsername(username);

            if (player != null) return player;

            // lookup in repository
            // create new Player(this, PlayerEntity);

            return null;
        }

        public async Task<IPlayer> CreatePlayer(int id, ISession session)
        {
            if ((id <= 0) || (session == null)) return null;

            IPlayer player = await GetOfflinePlayerById(id);

            if (player == null) return null;

            if (!player.SetSession(session))
            {
                await player.DisposeAsync();
                await session.DisposeAsync();

                return null;
            }

            if (!_players.TryAdd(id, player)) return null;

            await player.InitAsync();

            return player;
        }

        public async Task RemovePlayer(int id)
        {
            if (id <= 0) return;

            var player = GetPlayerById(id);

            if (player == null) return;

            _players.Remove(id, out var removedPlayer);

            await player.DisposeAsync();
        }

        public async Task RemoveAllPlayers()
        {
            foreach (int id in _players.Keys)
            {
                await RemovePlayer(id);
            }
        }

        public void ClearPlayerRoomStatus(IPlayer player)
        {
            if (player == null) return;

            _navigatorManager.ClearRoomStatus(player);
        }

        public async Task EnterRoom(IPlayer player, int roomId, string password = null, bool skipState = false, IPoint location = null)
        {
            await _navigatorManager.EnterRoom(player, roomId, password, skipState, location);
        }

        public async Task<string> GetPlayerName(int playerId)
        {
            var player = GetPlayerById(playerId);

            if (player != null) return player.Name;

            using var scope = _serviceScopeFactory.CreateScope();

            var playerRepository = scope.ServiceProvider.GetService<IPlayerRepository>();

            return (await playerRepository.FindUsernameAsync(playerId))?.Name ?? "";
        }

        public async Task<IList<IPlayerBadge>> GetPlayerActiveBadges(int playerId)
        {
            if (playerId <= 0) return null;

            var player = GetPlayerById(playerId);

            if (player == null)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var playerBadgeRepository = scope.ServiceProvider.GetService<IPlayerBadgeRepository>();

                var entities = await playerBadgeRepository.FindActiveByPlayerIdAsync(playerId);

                return (IList<IPlayerBadge>)entities;
            }

            return player.PlayerInventory?.BadgeInventory?.ActiveBadges;
        }
    }
}

﻿using System;
using Turbo.Database.Entities;
using Turbo.Packets.Sessions;

namespace Turbo.Players
{
    public class Player : IPlayer
    {
        private IPlayerContainer _playerContainer { get; set; }
        private ISession _session { get; set; }
        private bool _isDisposing { get; set; }

        public PlayerDetails PlayerDetails { get; private set; }

        public Player(IPlayerContainer playerContainer, PlayerEntity playerEntity)
        {
            _playerContainer = playerContainer;

            PlayerDetails = new PlayerDetails(this, playerEntity);
        }

        public bool SetSession(ISession session)
        {
            if ((_session != null) && (_session != session)) return false;

            if (!session.SetSessionPlayer(this)) return false;

            _session = session;

            return true;
        }

        public void Init()
        {
            // load roles
            // load inventory
            // load messenger
        }

        public void Dispose()
        {
            if (_isDisposing) return;

            _isDisposing = true;

            // remove assigned RoomObject

            if (_playerContainer != null)
            {
                _playerContainer.RemovePlayer(Id);
            }

            // set offline in PlayerDetails

            // dispose messenger
            // dispose inventory
            // dispose roles

            _session.Dispose();

            PlayerDetails.SaveNow();

            // save player details

        }

        public int Id
        {
            get
            {
                return PlayerDetails.Id;
            }
        }

        public string Name
        {
            get
            {
                return PlayerDetails.Name;
            }
        }
    }
}

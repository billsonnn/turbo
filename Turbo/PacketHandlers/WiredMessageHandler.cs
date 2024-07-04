using System;
using Turbo.Core.Game.Inventory;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.Packets;
using Turbo.Packets.Incoming.Wired;
using Turbo.Packets.Incoming.Inventory.Furni;
using Turbo.Packets.Incoming.Users;
using Turbo.Packets.Outgoing.Users;
using Turbo.Core.PacketHandlers;
using Turbo.Core.Game.Players;
using Turbo.Core.Packets.Messages;
using Turbo.Packets.Outgoing.Wired;
using Turbo.Core.Game.Wired;

namespace Turbo.Main.PacketHandlers
{
    public class WiredMessageHandler : IWiredMessageHandler
    {
        private readonly IPacketMessageHub _messageHub;

        public WiredMessageHandler(
            IPacketMessageHub messageHub)
        {
            _messageHub = messageHub;

            _messageHub.Subscribe<ApplySnapshotMessage>(this, OnApplySnapshotMessage);
            _messageHub.Subscribe<OpenWiredMessage>(this, OnOpenWiredMessage);
            _messageHub.Subscribe<UpdateActionMessage>(this, OnUpdateActionMessage);
            _messageHub.Subscribe<UpdateConditionMessage>(this, OnUpdateConditionMessage);
            _messageHub.Subscribe<UpdateTriggerMessage>(this, OnUpdateTriggerMessage);
        }

        protected virtual void OnApplySnapshotMessage(ApplySnapshotMessage message, ISession session)
        {
            if (session.Player == null) return;
        }

        protected virtual void OnOpenWiredMessage(OpenWiredMessage message, ISession session)
        {
            if (session.Player == null) return;
        }

        protected virtual void OnUpdateActionMessage(UpdateActionMessage message, ISession session)
        {
            UpdateWired(message, session);
        }

        protected virtual void OnUpdateConditionMessage(UpdateConditionMessage message, ISession session)
        {
            UpdateWired(message, session);
        }

        protected virtual void OnUpdateTriggerMessage(UpdateTriggerMessage message, ISession session)
        {
            UpdateWired(message, session);
        }

        protected virtual void UpdateWired(UpdateWired message, ISession session)
        {
            if (session.Player == null) return;

            var floorFurniture = session.Player.RoomObject?.Room?.RoomFurnitureManager?.GetFloorFurniture(message.ItemId);

            if (floorFurniture == null || floorFurniture.RoomObject == null || floorFurniture.RoomObject.Logic is not IFurnitureWiredLogic wiredLogic) return;

            var wiredData = wiredLogic.CreateWiredDataFromJson(null);

            if (wiredData == null) return;

            wiredData.SetFromMessage((IMessageEvent)message);

            if (wiredLogic.SaveWiredData(session.Player.RoomObject, wiredData))
            {
                session.Send(new WiredSavedMessage());
            }
        }
    }
}


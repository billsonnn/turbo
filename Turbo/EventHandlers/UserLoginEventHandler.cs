﻿using System.Threading.Tasks;
using Turbo.Core.EventHandlers;
using Turbo.Core.Events;
using Turbo.Events.Game.Security;
using Turbo.Packets.Incoming.Room.Action;
using Turbo.Packets.Incoming.Room.Chat;
using Turbo.Packets.Outgoing.Navigator;
using ChatMessage = Turbo.Packets.Outgoing.Room.Chat.ChatMessage;

namespace Turbo.EventHandlers;

public class UserLoginEventHandler : ILoginEventHandler
{
    
    private readonly ITurboEventHub _eventHub;
    
    public UserLoginEventHandler(ITurboEventHub eventHub)
    {
        _eventHub = eventHub;
        
        _eventHub.Subscribe<UserLoginEvent>(this, UserLoginEvent);
    }
    
    public void UserLoginEvent(UserLoginEvent user)
    {
        user.Player.Session.Send(new NavigatorSettingsMessage
        {  
            RoomIdToEnter = 1,
            HomeRoomId = 1
        });
    }
}
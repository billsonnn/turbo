﻿namespace Turbo.Packets.Incoming.Room.Layout
{
    public record UpdateFloorPropertiesMessage : IMessageEvent
    {
        public string Model { get; init; }
        public int DoorX { get; init; }
        public int DoorY { get; init; }
        public int DoorRotation { get; init; }
        public int WallThickness { get; init; }
        public int FloorThickness { get; init; }
        public int? WallHeight { get; init; }
    }
}

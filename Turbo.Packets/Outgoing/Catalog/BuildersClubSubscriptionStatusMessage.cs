using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.Catalog
{
    public record BuildersClubSubscriptionStatusMessage : IComposer
    {
        public int SecondsLeft { get; init; }
        public int FurniLimit { get; init; }
        public int MaxFurniLimit { get; init; }
        public int? SecondsLeftWithGrace { get; init; }
    }
}
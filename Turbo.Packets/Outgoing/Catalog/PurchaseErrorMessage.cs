using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turbo.Core.Game.Catalog.Constants;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.Catalog
{
    public record PurchaseErrorMessage : IComposer
    {
        public PurchaseErrorEnum ErrorCode { get; init; }
    }
}
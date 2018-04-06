using System;

namespace AkkaES.Core.EventSourcing
{
    public interface IAddressed
    {
        Guid RecipientId { get; }
    }
}
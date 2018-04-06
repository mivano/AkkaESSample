using System;

namespace AkkaES.Core.EventSourcing
{
    public sealed class GetState : ICommand
    {
        public readonly Guid Id;

        public GetState(Guid id)
        {
            Id = id;
        }
    }
}
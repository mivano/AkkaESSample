using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CustomerChangedEvent : IEvent
    {
        public Guid Id { get; }
        public string Name { get; }

        public CustomerChangedEvent(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class CustomerRemovedEvent : IEvent
    {
        public Guid Id { get; }

        public CustomerRemovedEvent(Guid id)
        {
            Id = id;
        }
    }
}
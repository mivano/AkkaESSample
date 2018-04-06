using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CustomerBaseCommand  : IAddressed, ICommand
    {
        public CustomerBaseCommand(Guid id)
        {
            Id = id;
        }
        public Guid Id { get; }

        public Guid RecipientId => Id;
    }
}
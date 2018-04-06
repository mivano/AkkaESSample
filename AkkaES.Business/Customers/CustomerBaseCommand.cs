using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CustomerBaseCommand  : IAddressed, ICommand
    {
        public Guid RecipientId { get; }
    }
}
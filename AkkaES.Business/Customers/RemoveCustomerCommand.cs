using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class RemoveCustomerCommand : CustomerBaseCommand
    {
        public RemoveCustomerCommand(Guid id) : base(id)
        {
        
        }

    }
}
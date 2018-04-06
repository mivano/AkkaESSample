using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class UpdateCustomerCommand : CustomerBaseCommand
    {
        public UpdateCustomerCommand(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
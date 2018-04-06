using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CreateCustomerCommand : ICommand
    {
        public CreateCustomerCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }
    }
}
using System;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CustomerEntity : IEntity<Guid>
    {
        public CustomerEntity(Guid id)
        {
            Id = id;
        }

        public string Name { get; set; }

        public Guid Id { get; set; }
    }
}
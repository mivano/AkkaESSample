using System;
using System.Collections.Generic;
using Akka;
using Akka.Actor;
using Akka.Persistence;
using Akka.Util.Internal;

namespace AkkaES.Business.Customers
{
    public class CustomersView : PersistentActor
    {
        public sealed class CustomerIndexed
        {
            public CustomerEntity Customer { get; }

            public CustomerIndexed(CustomerEntity customer)
            {
                Customer = customer;
            }
        }

        public sealed class CustomerRemovedFromIndex
        {
            public Guid Id { get; }

            public CustomerRemovedFromIndex(Guid id)
            {
                Id = id;
            }
        }

        public sealed class GetCustomers
        {

        }

        public class CustomerList
        {
            public IEnumerable<CustomerEntity> Customers { get; }

            public CustomerList(IEnumerable<CustomerEntity> customers)
            {
                Customers = customers;
            }
        }

        private IDictionary<Guid, CustomerEntity> _customers = new Dictionary<Guid, CustomerEntity>();
        private int _counter;

        public CustomersView()
        {
            Context.System.EventStream.Subscribe(Self, typeof(CustomerChangedEvent));
            Context.System.EventStream.Subscribe(Self, typeof(CustomerRemovedEvent));
        }

        protected override bool ReceiveRecover(object message)
        {
            return message.Match()
                .With<CustomerIndexed>(e =>
                {
                    _customers.AddOrSet(e.Customer.Id, e.Customer);
                })
                .With<CustomerRemovedEvent>(e =>
                {
                    _customers.Remove(e.Id);
                })
                .With<SnapshotOffer>(offer =>
                {
                    _customers = (Dictionary<Guid, CustomerEntity>)offer.Snapshot;
                })
                .WasHandled;
        }

        protected override bool ReceiveCommand(object message)
        {
            return message.Match()
                .With<CustomerChangedEvent>(customer =>
                {
                    Persist(new CustomerIndexed(new CustomerEntity(  customer.Id){Name= customer.Name }), e =>
                    {
                        _customers.AddOrSet(e.Customer.Id, e.Customer);

                        _counter++;
                        if (_counter % 5 == 0)
                        {
                            SaveSnapshot(_customers);

                            _counter = 0;
                        }
                    });
                })
                .With<CustomerRemovedEvent>(customer =>
                {
                    Persist(new CustomerRemovedFromIndex(customer.Id), e =>
                    {
                        _customers.Remove(e.Id);

                        _counter++;
                        if (_counter % 5 == 0)
                        {
                            SaveSnapshot(_customers);

                            _counter = 0;
                        }
                    });
                })
                .With<SaveSnapshotSuccess>(snapshotSaved =>
                {
                    var snapshotSeqNr = snapshotSaved.Metadata.SequenceNr;
                    // delete all messages from journal and snapshot store before latests confirmed
                    // snapshot, we won't need them anymore
                    DeleteMessages(snapshotSeqNr);
                    DeleteSnapshots(new SnapshotSelectionCriteria(snapshotSeqNr - 1));
                })
                .With<GetCustomers>(request =>
                {
                    Sender.Tell(new CustomerList(_customers.Values));
                })
                .WasHandled;
        }

        public override string PersistenceId => "customers";
    }
}
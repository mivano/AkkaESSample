using System;
using Akka;
using Akka.Actor;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{
    public class CustomerActor : AggregateRootActor<CustomerEntity, Guid>
    {
        private readonly Guid _id;

        public CustomerActor(Guid id) : base("customer-" + id.ToString("N"))
        {
            _id = id;
                      
            // start in uninitialized state
            Context.Become(Uninitialized);

        }

        private bool Uninitialized(object message)
        {
            return message.Match().With<CreateCustomerCommand>(c =>
            {
                Persist(new CustomerChangedEvent(_id, c.Name), Sender);
            }).WasHandled;
        }

        private bool Active(object message)
        {
            return base.ReceiveCommand(message);
        }


        protected override bool OnCommand(object message)
        {
            return false;
        }

        protected override void UpdateState(IEvent domainEvent, IActorRef sender)
        {

            domainEvent.Match()
                .With<CustomerChangedEvent>(e =>
                {
                    if (State == null)
                        State = new CustomerEntity(_id);

                    State.Name = e.Name;
                  
                    Context.Become(Active);

                    Log.Info("Registered customer {Name} with PersistenceId: {PersistenceId}", e.Name, e.Id);
                }) ;
        }

        protected override void OnReplaySuccess()
        {
            if (State == null) Become(Uninitialized);
            else Become(Active);
        }
    }
}
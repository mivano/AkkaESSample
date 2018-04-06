using System;
using Akka.Actor;
using AkkaES.Core.EventSourcing;

namespace AkkaES.Business.Customers
{

    public class CustomerCoordinator : AggregateCoordinator
    {
        public CustomerCoordinator() : base("customers")
        {
        }

        public override Props GetProps(Guid id)
        {
            return Props.Create(() => new CustomerActor(id));
        }

        protected override bool Receive(object message)
        {
            var handled = base.Receive(message);
            if (handled)
                return true;

            switch (message)
            {
                case CreateCustomerCommand cmd:
                    var id = cmd.Id;
                    ForwardCommand(id, message as CreateCustomerCommand);
                    break;
                case GetState _:
                    var getState = message as GetState;
                    ForwardCommand(getState.Id, getState);
                    break;
                case IAddressed _ when message is CustomerBaseCommand:
                    ForwardCommand((CustomerBaseCommand) message);
                    break;
                default:
                    return false;
            }

            return true;

        }
    }
}

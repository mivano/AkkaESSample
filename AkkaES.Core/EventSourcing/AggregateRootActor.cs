using System;
using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Akka.Persistence;

namespace AkkaES.Core.EventSourcing
{
    public abstract class AggregateRootActor<TEntity, TKey> : PersistentActor where TEntity : class, IEntity<TKey>
    {
        private static readonly TimeSpan DefaultReceiveTimeout = TimeSpan.FromMinutes(5);
        private readonly ILoggingAdapter _logger = Context.GetLogger<SerilogLoggingAdapter>();

        /// <summary>
        /// Maximum number of events to occur in a row before state will be snapshoted.
        /// This is useful in scenarios, when actor may have to recover it's from potentially large number of events.
        /// This way snapshots are made automatically after specific number of events become persisted.
        /// </summary>
        private const int MaxEventsToSnapshot = 10;  // in real life this should be greater number
        private int _eventsSinceLastSnapshot = 0;

        protected TEntity State { get; set; }

        private int _eventCount = 0;

        protected AggregateRootActor(string persistenceId)
        {
            // This will terminate the actor after the timeout as we might not want to have too many child entities running around.
            Context.SetReceiveTimeout(DefaultReceiveTimeout);

            PersistenceId = persistenceId;
        }

        protected override bool ReceiveRecover(object message)
        {
            switch (message)
            {
                case SnapshotOffer _:
                    if (((SnapshotOffer)message).Snapshot is TEntity offeredState)
                    {
                        State = offeredState;
                    }
                    break;
                case IEvent _:
                    UpdateState(message as IEvent, null);
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected override bool Receive(object message)
        {
            return base.Receive(message);
        }

        protected override bool ReceiveCommand(object message)
        {  
            if (message is GetState)
            {
                Sender.Tell(State, Self);
                return true;
            }

            if (message is ReceiveTimeout)
            {
                Context.Parent.Tell(AggregateCoordinator.Passivate.Instance);
                return true;
            }

            return OnCommand(message);
        }

        public override string PersistenceId { get; }

        protected override ILoggingAdapter Log =>  _logger;

        protected abstract bool OnCommand(object message);

        /// <summary>
        /// Wrapper method around persistence mechanisms. It persist an event, publishes it through event stream,
        /// calls aggregate state update and periodically performs snapshotting.
        /// </summary>
        protected void Persist(IEvent domainEvent, IActorRef sender = null)
        {
            Persist(domainEvent, e =>
            {
                UpdateState(domainEvent, sender);
                Publish(e);

                // if persisted events counter already exceeded the MaxEventsToSnapshot limit
                // snapshot will be automatically stored and counter will reset
                if ((_eventsSinceLastSnapshot++) >= MaxEventsToSnapshot)
                {
                    SaveSnapshot(State);
                    _eventsSinceLastSnapshot = 0;
                }

                _eventCount++;
            });
        }

        protected void Publish(IEvent domainEvent)
        {
            Context.System.EventStream.Publish(domainEvent);
        }

        /// <summary>
        /// Update state is used for changing actor's internal state in response to incoming events.
        /// This method should be idempotent and should never call event persisting methods itself 
        /// nor generating another commands.
        /// 
        /// While in recovering mode, <paramref name="sender"/> is always null.
        /// </summary>
        protected abstract void UpdateState(IEvent domainEvent, IActorRef sender);

    }
}
namespace AkkaES.Core.EventSourcing
{
    public interface IEntity<TIdentifier>
    {
        TIdentifier Id { get; set; }
    }
}
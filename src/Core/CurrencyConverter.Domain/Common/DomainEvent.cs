namespace CurrencyConverter.Domain.Common
{
    public abstract class DomainEvent
    {
        public DateTime Timestamp { get; }
        public Guid Id { get; }

        protected DomainEvent()
        {
            Timestamp = DateTime.UtcNow;
            Id = Guid.NewGuid();
        }
    }

}

namespace StatLight.Core.Events
{
    public abstract class PayloadEvent<TPayload>
    {
        public TPayload Payload { get; set; }
    }
}
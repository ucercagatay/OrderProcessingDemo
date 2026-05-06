namespace OrderService.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; }
    public string Payload { get; set; }
    public OutboxMessageStatus Status { get; private set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
        Status = OutboxMessageStatus.Pending;
        RetryCount = 0;
    }
    private  OutboxMessage()
    {
    }
    public void MarkAsPublished()
    {
        Status = OutboxMessageStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        Status = OutboxMessageStatus.Failed;
        RetryCount++;
    }
}
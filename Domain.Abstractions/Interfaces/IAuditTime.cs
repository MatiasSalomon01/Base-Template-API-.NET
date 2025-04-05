namespace Domain.Abstractions.Interfaces;

public interface IAuditTime
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
namespace Domain.Abstractions.Interfaces;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
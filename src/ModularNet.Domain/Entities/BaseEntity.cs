namespace ModularNet.Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public string? Meta { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
}
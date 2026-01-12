namespace Domain.Entities;

public class AccountCategory : BaseEntity
{
    public required string Name { get; set; }
    public required string UserId { get; set; }
}
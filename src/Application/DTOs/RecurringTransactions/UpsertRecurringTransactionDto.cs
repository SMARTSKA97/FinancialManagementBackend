using Domain.Enums;
using FluentValidation;

namespace Application.DTOs.RecurringTransactions;

public class UpsertRecurringTransactionDto
{
    public int? Id { get; set; }
    public int AccountId { get; set; }
    public int? TransactionCategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpsertRecurringTransactionDtoValidator : AbstractValidator<UpsertRecurringTransactionDto>
{
    public UpsertRecurringTransactionDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

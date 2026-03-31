using Application.Common.Models;
using Application.Contracts;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Subscriptions.Commands;

public class CreateSubscriptionDto
{
    public required string Name { get; set; }
    public decimal Amount { get; set; }
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public string? Tag { get; set; }
    public string? CancellationUrl { get; set; }
}

public record CreateSubscriptionCommand(string UserId, CreateSubscriptionDto Dto) : IRequest<Result<int>>;

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Amount).GreaterThan(0);
        RuleFor(x => x.Dto.AccountId).GreaterThan(0);
    }
}

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var recurringTransaction = new RecurringTransaction
        {
            UserId = request.UserId,
            AccountId = dto.AccountId,
            TransactionCategoryId = dto.CategoryId,
            Description = $"Subscription: {dto.Name}",
            Amount = dto.Amount,
            Type = TransactionType.Expense, // subscriptions are expenses
            Frequency = dto.Frequency,
            StartDate = dto.StartDate,
            NextProcessDate = dto.StartDate,
            IsActive = true
        };

        var subscription = new Subscription
        {
            UserId = request.UserId,
            Name = dto.Name,
            Tag = dto.Tag,
            CancellationUrl = dto.CancellationUrl,
            RecurringTransaction = recurringTransaction // EF will wire up the FK
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(subscription.Id);
    }
}

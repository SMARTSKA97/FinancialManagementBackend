using FluentValidation;
using FinancialPlanner.Application.DTOs.Auth;
using FinancialPlanner.Application.DTOs.Transactions;

namespace FinancialPlanner.Application.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6)
            .Matches("[A-Z]").WithMessage("Password must contain one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain one number.");
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.");
    }
}

public class UpsertTransactionDtoValidator : AbstractValidator<UpsertTransactionDto>
{
    public UpsertTransactionDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
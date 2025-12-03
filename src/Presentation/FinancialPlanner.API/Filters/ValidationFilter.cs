using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinancialPlanner.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Iterate through all arguments in the action method
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            // 2. Check if there is a validator registered for this argument's type
            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            
            // 3. Resolve the validator from the service provider
            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                // 4. Validate the object
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    // 5. Return 400 Bad Request with errors if invalid
                    var errors = validationResult.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    context.Result = new BadRequestObjectResult(new
                    {
                        IsSuccess = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                    return;
                }
            }
        }

        await next();
    }
}

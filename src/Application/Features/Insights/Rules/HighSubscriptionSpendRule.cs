using Domain.Rules;

namespace Application.Features.Insights.Rules;

public class HighSubscriptionSpendRule : IFinancialRule
{
    public string RuleName => "High Subscription Spend";
    public string Description => "Warns if subscriptions exceed 15% of income.";

    public FinancialInsight Evaluate(RuleContext context)
    {
        if (context.TotalIncome == 0) return new FinancialInsight { Triggered = false, Title = "", Message = "" };

        var subRatio = context.SubscriptionSpend / context.TotalIncome;
        if (subRatio > 0.15m)
        {
            return new FinancialInsight
            {
                Title = "High Subscription Spend 🚨",
                Message = $"You spend {(subRatio * 100):F1}% of your income on subscriptions. Consider trimming unused ones.",
                Type = InsightType.Warning,
                Triggered = true
            };
        }

        return new FinancialInsight { Triggered = false, Title = "", Message = "" };
    }
}

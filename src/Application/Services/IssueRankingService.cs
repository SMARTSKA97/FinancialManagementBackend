using Domain.Entities;

namespace Application.Services;

public class IssueRankingService
{
    // Weights
    private const int ImpactMoneyWeight = 100;
    private const int ImpactNoMoneyWeight = 10;
    
    private const int FreqAlwaysWeight = 10;
    private const int FreqFrequentWeight = 5;
    private const int FreqRareWeight = 1;
    
    private const int SevCriticalWeight = 5;
    private const int SevMajorWeight = 3;
    private const int SevMinorWeight = 1;

    public double CalculatePainScore(Issue issue)
    {
        double impactScore = issue.ImpactsMoney ? ImpactMoneyWeight : ImpactNoMoneyWeight;
        
        double freqScore = issue.Frequency switch
        {
            "Always" => FreqAlwaysWeight,
            "Frequent" => FreqFrequentWeight,
            _ => FreqRareWeight
        };

        double sevScore = issue.Severity switch
        {
            "Critical" => SevCriticalWeight,
            "Major" => SevMajorWeight,
            _ => SevMinorWeight
        };

        double baseScore = (impactScore * freqScore * sevScore);
        
        // Add TrustPenalty (if users are angry/churning)
        baseScore += issue.TrustPenalty;
        
        // Add Financial Risk directly to score? Or scaled? 
        // User said: (Impact x Frequency x Severity) + TrustPenalty + FinancialRisk
        // Let's assume FinancialRisk is the actual amount or a scaled value.
        // If amount is 1000, adding 1000 might skew it. Let's optimize:
        // If FinancialImpactAmount is present, we treat it as a high priority booster.
        
        if (issue.FinancialImpactAmount.HasValue && issue.FinancialImpactAmount.Value > 0)
        {
            // Log scale to prevent millions from breaking the score, or linear if "money is money"
            // Simple approach: Add 1 point per 10 currency units, capped? 
            // Or just add the raw amount if the formula implies "Total ₹ amount impacted"
            // "Final Score = ... + FinancialRisk"
            // Let's just add it, maybe scaled by 0.1 to normalize with other scores being in 100s.
            // Actually, let's stick to the prompt's implied simple addition.
            baseScore += (double)issue.FinancialImpactAmount.Value;
        }

        // Add Votes? Prompt said "Upvotes are lazy. You need weighted pain."
        // But maybe small weight for votes.
        // The prompt formula didn't explicitly include votes in the main multiplication, 
        // but "Issue Ranking 2.0 (Not just upvotes)" implies we replace upvotes or Combine.
        // Let's leave votes out of the *PainScore* or add as a tie-breaker.
        // For now, adhere strictly to the formula in prompt.
        
        return baseScore;
    }
}

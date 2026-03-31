using Application.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Services;

public class IssueSimilarityService
{
    private readonly IApplicationDbContext _context;

    public IssueSimilarityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Issue>> FindSimilarIssuesAsync(string title, string description)
    {
        // Simple heuristic for MVP:
        // 1. Text match on Title (Contains)
        // 2. Normalize text?
        
        // Ideally use pg_trgm or full-text, but standard LINQ 'Contains' is safe fallback.
        var terms = title.Split(' ').Where(t => t.Length > 3).Take(3).ToList();
        
        if (!terms.Any()) return new List<Issue>();

        var query = _context.Issues.AsQueryable();
        
        // Build a dynamic OR predicate or just memory filter top recent
        // For EF Core, we can't easily do "Where(x => terms.Any(t => x.Title.Contains(t)))" strictly translated.
        // So we might fetch recent active issues and filter in memory if dataset is small, 
        // OR just search for the first keyword.
        
        var firstTerm = terms.First();
        var similar = await query
            .Where(i => i.Title.Contains(firstTerm) || i.Description.Contains(firstTerm))
            .OrderByDescending(i => i.UpdatedAt)
            .Take(20)
            .ToListAsync();

        return similar;
    }
}

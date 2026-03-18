using Application.Contracts;
using Application.DTOs.Issue;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IssueRankingService _rankingService;
    private readonly IssueSimilarityService _similarityService;

    public IssuesController(IApplicationDbContext context, IssueRankingService rankingService, IssueSimilarityService similarityService)
    {
        _context = context;
        _rankingService = rankingService;
        _similarityService = similarityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<IssueDto>>> GetIssues([FromQuery] string? status, [FromQuery] string? sort = "pain")
    {
        var query = _context.Issues
            .Include(i => i.Category)
            .Include(i => i.Subcategory)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (sort == "pain")
        {
            query = query.OrderByDescending(i => i.PainScore);
        }
        else
        {
            query = query.OrderByDescending(i => i.CreatedAt);
        }

        var issues = await query.Take(50).ToListAsync();

        var dtos = issues.Select(i => new IssueDto
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            Status = i.Status,
            PainScore = i.PainScore,
            CategoryName = i.Category?.Name ?? "Uncategorized",
            SubcategoryName = i.Subcategory?.Name ?? "",
            CreatedAt = i.CreatedAt,
            Votes = i.Votes,
            CommentCount = 0 // Needs count loaded
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("check-similar")]
    public async Task<ActionResult<List<IssueDto>>> CheckSimilar([FromBody] CreateIssueDto input)
    {
        var similar = await _similarityService.FindSimilarIssuesAsync(input.Title, input.Description);
        return Ok(similar.Select(i => new IssueDto { 
            Id = i.Id, 
            Title = i.Title, 
            Description = i.Description,
            Status = i.Status,
            PainScore = i.PainScore,
            CategoryName = i.Category?.Name ?? "Similar Match", 
            CreatedAt = i.CreatedAt
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateIssueDto input)
    {
        var issue = new Domain.Entities.Issue
        {
            Title = input.Title,
            Description = input.Description,
            Priority = input.Priority,
            CategoryId = input.CategoryId,
            SubcategoryId = input.SubcategoryId,
            Severity = input.Severity,
            ImpactsMoney = input.ImpactsMoney,
            FinancialImpactAmount = input.FinancialImpactAmount,
            Frequency = input.Frequency,
            Status = "New"
        };
        
        issue.PainScore = _rankingService.CalculatePainScore(issue);

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        return Ok(issue.Id);
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult> AddComment(int id, [FromBody] CreateCommentDto input)
    {
        var comment = new IssueComment
        {
            IssueId = id,
            Content = input.Content,
            StructuredMetadata = input.StructuredMetadata,
            IsHelpful = false
        };
        
        _context.IssueComments.Add(comment);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("seed")]
    public async Task<ActionResult> SeedTaxonomy([FromServices] TaxonomySeederService seeder)
    {
        await seeder.SeedAsync();
        return Ok("Taxonomy seeded.");
    }

    [HttpGet("taxonomy")]
    public async Task<ActionResult<List<IssueTaxonomy>>> GetTaxonomies()
    {
        var list = await _context.IssueTaxonomies
            .Include(t => t.Children)
            .Where(t => t.ParentId == null) // Root categories
            .ToListAsync();
        return Ok(list);
    }
}


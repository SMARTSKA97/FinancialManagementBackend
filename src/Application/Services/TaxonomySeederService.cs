using Domain.Entities;
using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class TaxonomySeederService
{
    private readonly IApplicationDbContext _context;

    public TaxonomySeederService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.IssueTaxonomies.AnyAsync()) return;

        var categories = new Dictionary<string, List<(string Sub, string[] Contexts)>>
        {
            { "Financial Core", new List<(string, string[])> {
                ("Balance mismatch", new[] { "Bank Sync", "Manual Entry" }),
                ("Duplicate transaction", new[] { "Import", "Sync" }),
                ("Missing transaction", new[] { "Sync" }),
                ("Wrong category", new[] { "Auto-cat", "Rules" }),
                ("Incorrect tax calculation", new[] { "Reports" })
            }},
            { "UX / Flow", new List<(string, string[])> {
                ("Too many clicks", new[] { "Navigation", "Forms" }),
                ("Confusing terminology", new[] { "Labels", "Help" }),
                ("Poor mobile layout", new[] { "Dashboard", "Transaction List" }),
                ("Accessibility issue", new[] { "Colors", "Screen Reader" })
            }},
            { "Performance", new List<(string, string[])> {
                ("Slow dashboard", new[] { "Load Time" }),
                ("Report takes too long", new[] { "Export", "View" }),
                ("App freeze", new[] { "Startup", "Scrolling" })
            }},
            { "Data Trust", new List<(string, string[])> {
                ("Data mismatch across screens", new[] { "Dashboard vs Reports" }),
                ("Export mismatch", new[] { "CSV", "PDF" }),
                ("Audit trail missing", new[] { "History" })
            }},
            { "Security / Privacy", new List<(string, string[])> {
                ("Session timeout", new[] { "Login" }),
                ("Data visibility issue", new[] { "Shared Access" }),
                ("Export access issue", new[] { "Download" })
            }}
        };

        foreach (var cat in categories)
        {
            var categoryEntity = new IssueTaxonomy { Name = cat.Key, Type = "Category" };
            _context.IssueTaxonomies.Add(categoryEntity);
            await _context.SaveChangesAsync(); // Save to get ID

            foreach (var sub in cat.Value)
            {
                var subEntity = new IssueTaxonomy { Name = sub.Item1, Type = "Subcategory", ParentId = categoryEntity.Id };
                _context.IssueTaxonomies.Add(subEntity);
                await _context.SaveChangesAsync();

                foreach (var ctx in sub.Item2)
                {
                    _context.IssueTaxonomies.Add(new IssueTaxonomy { Name = ctx, Type = "Context", ParentId = subEntity.Id });
                }
            }
        }
        await _context.SaveChangesAsync();
    }
}

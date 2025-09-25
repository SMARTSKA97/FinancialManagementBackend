using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IGenericRepository<Category> _categoryRepository;

    public CategoriesController(IGenericRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var allCategories = await _categoryRepository.GetAllAsync();
        var userCategories = allCategories.Where(c => c.UserId == userId);
        return Ok(userCategories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryNameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = new Category
        {
            Name = dto.Name,
            UserId = userId!
        };
        var newCategory = await _categoryRepository.AddAsync(category);
        return CreatedAtAction(nameof(GetCategories), new { id = newCategory.Id }, newCategory);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryNameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
        {
            return NotFound();
        }
        category.Name = dto.Name;
        _categoryRepository.Update(category);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
        {
            return NotFound();
        }
        _categoryRepository.Delete(category);
        return NoContent();
    }
}

// A simple DTO for the category name
public class CategoryNameDto
{
    public required string Name { get; set; }
}
using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class AccountCategoriesController : BaseController
{
    // --- THIS IS THE FIX ---
    // We must inject the SERVICE, not the repository.
    private readonly ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto> _accountCategoryService;

    public AccountCategoriesController(ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto> accountCategoryService)
    {
        _accountCategoryService = accountCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Call the service
        var response = await _accountCategoryService.GetAllAsync(UserId);
        return HandleApiResponse(response);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertAccountCategoryDto dto)
    {
        // Call the service (this will now execute the try...catch block)
        var response = await _accountCategoryService.UpsertAsync(UserId, dto);
        return HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Call the service
        var response = await _accountCategoryService.DeleteAsync(UserId, id);
        return HandleApiResponse(response);
    }
}
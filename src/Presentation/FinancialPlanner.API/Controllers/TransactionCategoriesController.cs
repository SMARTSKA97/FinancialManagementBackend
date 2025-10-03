using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class TransactionCategoriesController : BaseController
{
    private readonly ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto> _transactionCategoryService;

    public TransactionCategoriesController(ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto> transactionCategoryService)
    {
        _transactionCategoryService = transactionCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _transactionCategoryService.GetAllAsync(UserId);
        return HandleApiResponse(response);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertTransactionCategoryDto dto)
    {
        var response = await _transactionCategoryService.UpsertAsync(UserId, dto);
        return HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _transactionCategoryService.DeleteAsync(UserId, id);
        return HandleApiResponse(response);
    }
}
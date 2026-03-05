using Application;
using Application.Contracts;
using Application.DTOs;
using Application.DTOs.Categories;
using Application.DTOs.TransactionCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TransactionCategoriesController : BaseController
{
    private readonly ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto> _transactionCategoryService;

    public TransactionCategoriesController(ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto> transactionCategoryService)
    {
        _transactionCategoryService = transactionCategoryService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> GetPaged([FromBody] QueryParameters queryParams)
    {
        var result = await _transactionCategoryService.GetPagedAsync(UserId!, queryParams);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _transactionCategoryService.GetAllAsync(UserId!);
        return HandleResult(result);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertTransactionCategoryDto dto)
    {
        var result = await _transactionCategoryService.UpsertAsync(UserId!, dto);
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteDto dto)
    {
        var result = await _transactionCategoryService.DeleteAsync(UserId!, dto.Id);
        return HandleResult(result);
    }
}

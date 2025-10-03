using AutoMapper;
using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class TransactionCategoriesController : BaseController
{
    private readonly IGenericRepository<TransactionCategory> _repository;
    private readonly IMapper _mapper;

    public TransactionCategoriesController(IGenericRepository<TransactionCategory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _repository.FindAsync(c => c.UserId == UserId);
        var categoryDtos = _mapper.Map<IReadOnlyList<TransactionCategoryDto>>(categories);
        return HandleApiResponse(ApiResponse<IReadOnlyList<TransactionCategoryDto>>.Success(categoryDtos));
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertTransactionCategoryDto dto)
    {
        TransactionCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            var existingCategory = await _repository.GetByIdAsync(dto.Id.Value);
            if (existingCategory == null || existingCategory.UserId != UserId)
            {
                return HandleApiResponse(ApiResponse<TransactionCategoryDto>.Failure("Category not found."));
            }
            category = _mapper.Map(dto, existingCategory);
        }
        else
        {
            category = _mapper.Map<TransactionCategory>(dto);
            category.UserId = UserId;
        }

        var savedCategory = await _repository.UpsertAsync(category);
        var resultDto = _mapper.Map<TransactionCategoryDto>(savedCategory);
        return HandleApiResponse(ApiResponse<TransactionCategoryDto>.Success(resultDto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null || category.UserId != UserId)
        {
            return HandleApiResponse(ApiResponse<bool>.Failure("Category not found."));
        }

        await _repository.DeleteAsync(category);
        return HandleApiResponse(ApiResponse<bool>.Success(true, "Category deleted successfully."));
    }
}

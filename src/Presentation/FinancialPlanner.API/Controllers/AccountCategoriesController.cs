using AutoMapper;
using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class AccountCategoriesController : BaseController
{
    private readonly IGenericRepository<AccountCategory> _repository;
    private readonly IMapper _mapper;

    public AccountCategoriesController(IGenericRepository<AccountCategory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _repository.FindAsync(c => c.UserId == UserId);
        var categoryDtos = _mapper.Map<IReadOnlyList<AccountCategoryDto>>(categories);
        return HandleApiResponse(ApiResponse<IReadOnlyList<AccountCategoryDto>>.Success(categoryDtos));
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertAccountCategoryDto dto)
    {
        AccountCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            category = await _repository.GetByIdAsync(dto.Id.Value);
            if (category == null || category.UserId != UserId)
            {
                return HandleApiResponse(ApiResponse<AccountCategoryDto>.Failure("Category not found."));
            }
            _mapper.Map(dto, category);
        }
        else
        {
            category = _mapper.Map<AccountCategory>(dto);
            category.UserId = UserId;
        }

        var savedCategory = await _repository.UpsertAsync(category);
        var resultDto = _mapper.Map<AccountCategoryDto>(savedCategory);
        return HandleApiResponse(ApiResponse<AccountCategoryDto>.Success(resultDto));
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
using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class AccountsController : BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("search")] // Paginated view
    public async Task<IActionResult> GetPaged([FromBody] QueryParameters queryParams)
    {
        var response = await _accountService.GetPagedAccountsAsync(UserId, queryParams);
        return HandleApiResponse(response);
    }

    [HttpPost("upsert")] // Create and Update
    public async Task<IActionResult> Upsert([FromBody] UpsertAccountDto dto)
    {
        var response = await _accountService.UpsertAccountAsync(UserId, dto);
        return HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _accountService.DeleteAccountAsync(UserId, id);
        return HandleApiResponse(response);
    }
}

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
        var result = await _accountService.GetPagedAccountsAsync(UserId, queryParams);
        return HandleResult(result);
    }

    [HttpPost("upsert")] // Create and Update
    public async Task<IActionResult> Upsert([FromBody] UpsertAccountDto dto)
    {
        var result = await _accountService.UpsertAccountAsync(UserId, dto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _accountService.DeleteAccountAsync(UserId, id);
        return HandleResult(result);
    }
}

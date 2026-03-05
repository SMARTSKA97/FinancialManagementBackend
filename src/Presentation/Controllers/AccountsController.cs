using Application;
using Application.Contracts;
using Application.DTOs;
using Application.DTOs.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize]
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

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteDto dto)
    {
        var result = await _accountService.DeleteAccountAsync(UserId, dto.Id);
        return HandleResult(result);
    }
}

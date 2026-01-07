using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/accounts/{accountId}/[controller]")]
public class TransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> GetPaged(int accountId, [FromBody] QueryParameters queryParams)
    {
        var result = await _transactionService.GetTransactionsAsync(UserId, accountId, queryParams);
        return HandleResult(result);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(int accountId, [FromBody] UpsertTransactionDto dto)
    {
        var result = await _transactionService.UpsertTransactionAsync(UserId, accountId, dto);
        return HandleResult(result);
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> Delete(int accountId, int transactionId)
    {
        var result = await _transactionService.DeleteTransactionAsync(UserId, accountId, transactionId);
        return HandleResult(result);
    }
    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer(int accountId, [FromBody] CreateTransferDto dto)
    {
        var result = await _transactionService.CreateTransferAsync(UserId, accountId, dto);
        return HandleResult(result);
    }

    [HttpPost("{transactionId}/switch-account")]
    public async Task<IActionResult> SwitchAccount(int accountId, int transactionId, [FromBody] SwitchTransactionAccountDto dto)
    {
        // We can ignore the 'accountId' from the URL as the transactionId is the true source of truth
        var result = await _transactionService.SwitchAccountAsync(UserId, transactionId, dto.DestinationAccountId);
        return HandleResult(result);
    }
}

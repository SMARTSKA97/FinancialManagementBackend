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
        var response = await _transactionService.GetTransactionsAsync(UserId, accountId, queryParams);
        return HandleApiResponse(response);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(int accountId, [FromBody] UpsertTransactionDto dto)
    {
        var response = await _transactionService.UpsertTransactionAsync(UserId, accountId, dto);
        return HandleApiResponse(response);
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> Delete(int accountId, int transactionId)
    {
        var response = await _transactionService.DeleteTransactionAsync(UserId, accountId, transactionId);
        return HandleApiResponse(response);
    }
    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer(int accountId, [FromBody] CreateTransferDto dto)
    {
        var response = await _transactionService.CreateTransferAsync(UserId, accountId, dto);
        return HandleApiResponse(response);
    }
}

using Application;
using Application.Contracts;
using Application.DTOs.Transactions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class TransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> GetPaged([FromBody] TransactionSearchDto dto)
    {
        var result = await _transactionService.GetTransactionsAsync(UserId, dto.AccountId, dto.QueryParameters);
        return HandleResult(result);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertTransactionPayloadDto dto)
    {
        var result = await _transactionService.UpsertTransactionAsync(UserId, dto.AccountId, dto.Transaction);
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteTransactionDto dto)
    {
        var result = await _transactionService.DeleteTransactionAsync(UserId, dto.AccountId, dto.TransactionId);
        return HandleResult(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferPayloadDto dto)
    {
        var result = await _transactionService.CreateTransferAsync(UserId, dto.AccountId, dto.Transfer);
        return HandleResult(result);
    }

    [HttpPost("switch-account")]
    public async Task<IActionResult> SwitchAccount([FromBody] SwitchTransactionAccountPayloadDto dto)
    {
        var result = await _transactionService.SwitchAccountAsync(UserId, dto.TransactionId, dto.DestinationAccountId);
        return HandleResult(result);
    }
}

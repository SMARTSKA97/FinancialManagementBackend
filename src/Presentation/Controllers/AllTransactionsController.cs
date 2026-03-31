using Application;
using Application.Contracts;
using Application.DTOs.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AllTransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;

    public AllTransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] QueryParameters queryParams)
    {
        var result = await _transactionService.GetAllTransactionsAsync(UserId, queryParams);
        return HandleResult(result);
    }
}

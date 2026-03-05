using Application.DTOs.Transactions;

namespace Application.DTOs.Transactions;

public class TransactionSearchDto
{
    public int AccountId { get; set; }
    public QueryParameters QueryParameters { get; set; } = new();
}

public class UpsertTransactionPayloadDto
{
    public int AccountId { get; set; }
    public required UpsertTransactionDto Transaction { get; set; }
}

public class DeleteTransactionDto
{
    public int AccountId { get; set; }
    public int TransactionId { get; set; }
}

public class CreateTransferPayloadDto
{
    public int AccountId { get; set; }
    public required CreateTransferDto Transfer { get; set; }
}

public class SwitchTransactionAccountPayloadDto
{
    public int TransactionId { get; set; }
    public int DestinationAccountId { get; set; }
}

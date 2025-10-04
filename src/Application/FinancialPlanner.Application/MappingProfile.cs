using AutoMapper;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Accounts;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.Feedback;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account Mappings
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.AccountCategoryName, opt => opt.MapFrom(src => src.AccountCategory != null ? src.AccountCategory.Name : ""));
        CreateMap<UpsertAccountDto, Account>();

        // Transaction Mappings
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.TransactionCategory != null ? src.TransactionCategory.Name : null));
        CreateMap<UpsertTransactionDto, Transaction>();

        // Category Mappings
        CreateMap<AccountCategory, AccountCategoryDto>();
        CreateMap<UpsertAccountCategoryDto, AccountCategory>();
        CreateMap<TransactionCategory, TransactionCategoryDto>();
        CreateMap<UpsertTransactionCategoryDto, TransactionCategory>();

        // Feedback Mappings
        CreateMap<CreateFeedbackDto, Feedback>();
    }
}
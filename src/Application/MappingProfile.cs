using AutoMapper;
using Application.DTOs.AccountCategory;
using Application.DTOs.Accounts;
using Application.DTOs.Categories;
using Application.DTOs.Feedback;
using Application.DTOs.TransactionCategory;
using Application.DTOs.Transactions;
using Domain.Entities;

namespace Application;

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
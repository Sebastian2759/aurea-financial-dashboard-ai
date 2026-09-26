using Application.Base;
using Application.Dtos;
using Application.Security;
using Application.Auditing;
using Domain.Base.Interface;
using Domain.Entities;
using Domain.Markets;
using Domain.Security;
using Domain.Contracts.Security;
using Domain.Contracts.Persistence;
using Domain.Contracts.Adapter.Mapper;
using Domain.Contracts.Adapter.MarketData;
using FluentValidation;
using MediatR;
namespace Application.UseCases.Markets.GetAssetHistory;
public sealed class GetAssetHistoryValidator : AbstractValidator<GetAssetHistoryRequest>
{
    public GetAssetHistoryValidator()
    {
        RuleFor(x => x.Currency).Must(MarketCatalog.HasCurrency);
        RuleFor(x => x.CoinId).Must(MarketCatalog.HasAsset);
        RuleFor(x => x.Days).Must(d => d is 1 or 7);
    }
}

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
namespace Application.UseCases.Markets.GetMarketSnapshot;
public sealed class GetMarketSnapshotValidator : AbstractValidator<GetMarketSnapshotRequest>
{
    public GetMarketSnapshotValidator()
    {
        RuleFor(x => x.Currency).Must(MarketCatalog.HasCurrency).WithMessage("Moneda no soportada.");
    }
}

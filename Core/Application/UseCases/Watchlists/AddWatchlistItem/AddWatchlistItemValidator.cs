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
namespace Application.UseCases.Watchlists.AddWatchlistItem;
public sealed class AddWatchlistItemValidator : AbstractValidator<AddWatchlistItemRequest>
{
    public AddWatchlistItemValidator()
    {
        RuleFor(x => x.OwnerId).Must(id => DemoUsers.Find(id) is { Role: "Trader" or "Admin" }).WithMessage("Propietario no válido.");
        RuleFor(x => x.CoinId).Must(MarketCatalog.HasAsset).WithMessage("Activo no soportado.");
        RuleFor(x => x.Note).MaximumLength(240);
    }
}

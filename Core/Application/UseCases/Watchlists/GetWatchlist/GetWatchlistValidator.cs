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
namespace Application.UseCases.Watchlists.GetWatchlist;
public sealed class GetWatchlistValidator : AbstractValidator<GetWatchlistRequest>
{
    public GetWatchlistValidator()
    {
        RuleFor(x => x.OwnerId).Must(id => DemoUsers.Find(id) is { Role: "Trader" or "Admin" }).WithMessage("Propietario no válido.");
    }
}

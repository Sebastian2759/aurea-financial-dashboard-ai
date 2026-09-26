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
namespace Application.UseCases.AuditEvents.GetAuditEvents;
public sealed class GetAuditEventsValidator : AbstractValidator<GetAuditEventsRequest>
{
    public GetAuditEventsValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ActorId).MaximumLength(64);
        RuleFor(x => x.Action).MaximumLength(64);
    }
}

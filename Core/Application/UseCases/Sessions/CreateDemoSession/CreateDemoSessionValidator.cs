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
namespace Application.UseCases.Sessions.CreateDemoSession;
public sealed class CreateDemoSessionValidator : AbstractValidator<CreateDemoSessionRequest>
{
    public CreateDemoSessionValidator()
    {
        RuleFor(x => x.UserId).Must(id => DemoUsers.Find(id) is not null).WithMessage("Usuario demo desconocido.");
    }
}

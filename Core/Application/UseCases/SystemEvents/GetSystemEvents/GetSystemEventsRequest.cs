using Application.Base;
using MediatR;
namespace Application.UseCases.SystemEvents.GetSystemEvents;
public sealed record GetSystemEventsRequest(int Page = 1, int PageSize = 20, string? Level = null, string? Component = null, DateTimeOffset? From = null, DateTimeOffset? To = null) : IRequest<ResponseBase<GetSystemEventsResponse>>;

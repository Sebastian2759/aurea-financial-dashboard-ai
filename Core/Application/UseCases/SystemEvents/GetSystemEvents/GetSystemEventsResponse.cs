using Application.Dtos;
namespace Application.UseCases.SystemEvents.GetSystemEvents;
public sealed record GetSystemEventsResponse(IReadOnlyList<SystemEventDto> Items, int Total, int Page, int PageSize);

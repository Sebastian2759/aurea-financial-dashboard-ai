using FluentValidation;
namespace Application.UseCases.SystemEvents.GetSystemEvents;
public sealed class GetSystemEventsValidator : AbstractValidator<GetSystemEventsRequest>
{
    public GetSystemEventsValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Level).Must(x => string.IsNullOrEmpty(x) || x is "Information" or "Warning" or "Error");
        RuleFor(x => x.Component).Must(x => string.IsNullOrEmpty(x) || x is "Api" or "Market");
        RuleFor(x => x.To).Must((request, to) => !request.From.HasValue || !to.HasValue || to >= request.From)
            .WithMessage("La fecha final debe ser igual o posterior a la inicial.");
    }
}

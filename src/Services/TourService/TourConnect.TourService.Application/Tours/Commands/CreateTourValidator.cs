using FluentValidation;

namespace TourConnect.TourService.Application.Tours.Commands;

public class CreateTourValidator : AbstractValidator<CreateTourCommand>
{
    public CreateTourValidator()
    {
        RuleFor(x => x.Dto.OperatorId).NotEmpty();
        RuleFor(x => x.Dto.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Dto.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Dto.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.DurationInHours).GreaterThan(0);
        RuleFor(x => x.Dto.BasePrice).GreaterThan(0);
    }
}

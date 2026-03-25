using FluentValidation;

namespace TourConnect.TourService.Application.Operators.Commands;

public class CreateOperatorValidator : AbstractValidator<CreateOperatorCommand>
{
    public CreateOperatorValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.Location).NotEmpty().MaximumLength(200);
    }
}

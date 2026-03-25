using FluentValidation;

namespace TourConnect.MatchingService.Application.Partners.Commands;

public class CreatePartnerValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.ContactEmail).NotEmpty().EmailAddress().MaximumLength(300);
        RuleFor(x => x.Dto.Location).NotEmpty().MaximumLength(200);
    }
}

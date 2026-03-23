using FluentValidation;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Validators;

public class CreatePartnerValidator : AbstractValidator<CreatePartnerRequest>
{
    public CreatePartnerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Partner adı boş olamaz.");

        // EmailAddress() → geçerli email formatı kontrolü (@ ve domain var mı?)
        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Email boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(x => x.Location).NotEmpty().WithMessage("Konum boş olamaz.");
    }
}

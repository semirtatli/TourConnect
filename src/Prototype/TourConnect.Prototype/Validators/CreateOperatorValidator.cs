using FluentValidation;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Validators;

public class CreateOperatorValidator : AbstractValidator<CreateOperatorRequest>
{
    public CreateOperatorValidator()
    {
        // NotEmpty → null veya boş string kabul etme
        RuleFor(x => x.Name).NotEmpty().WithMessage("Operatör adı boş olamaz.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Telefon numarası boş olamaz.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Konum boş olamaz.");
    }
}

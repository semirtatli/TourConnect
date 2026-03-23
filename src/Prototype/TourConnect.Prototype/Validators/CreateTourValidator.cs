using FluentValidation;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Validators;

public class CreateTourValidator : AbstractValidator<CreateTourRequest>
{
    public CreateTourValidator()
    {
        RuleFor(x => x.OperatorId).NotEmpty().WithMessage("OperatorId boş olamaz.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Tur başlığı boş olamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş olamaz.");
        RuleFor(x => x.DurationInHours).GreaterThan(0).WithMessage("Süre en az 1 saat olmalı.");
        RuleFor(x => x.BasePrice).GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalı.");
    }
}

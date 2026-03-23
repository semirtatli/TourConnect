using FluentValidation;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Validators;

public class CreateReservationValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.DealId).NotEmpty().WithMessage("DealId boş olamaz.");
        RuleFor(x => x.PartnerId).NotEmpty().WithMessage("PartnerId boş olamaz.");
        RuleFor(x => x.GuestName).NotEmpty().WithMessage("Misafir adı boş olamaz.");
        RuleFor(x => x.GuestCount).GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalı.");
    }
}

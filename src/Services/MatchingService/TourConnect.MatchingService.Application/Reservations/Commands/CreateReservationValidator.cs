using FluentValidation;

namespace TourConnect.MatchingService.Application.Reservations.Commands;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.Dto.DealId).NotEmpty();
        RuleFor(x => x.Dto.PartnerId).NotEmpty();
        RuleFor(x => x.Dto.GuestName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.GuestCount).GreaterThan(0);
    }
}

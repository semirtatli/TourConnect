using FluentValidation;

namespace TourConnect.TourService.Application.Deals.Commands;

public class CreateDealValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealValidator()
    {
        RuleFor(x => x.Dto.TourId).NotEmpty();
        RuleFor(x => x.Dto.AvailableSlots).GreaterThan(0);
        RuleFor(x => x.Dto.OriginalPrice).GreaterThan(0);
        RuleFor(x => x.Dto.DiscountedPrice).GreaterThan(0);
        RuleFor(x => x.Dto.ExpiresAt).GreaterThan(DateTime.UtcNow);
    }
}

using FluentValidation;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Validators;

public class CreateDealValidator : AbstractValidator<CreateDealRequest>
{
    public CreateDealValidator()
    {
        RuleFor(x => x.TourId).NotEmpty().WithMessage("TourId boş olamaz.");

        // GreaterThan(0) → en az 1 slot olmalı
        RuleFor(x => x.AvailableSlots).GreaterThan(0).WithMessage("En az 1 slot olmalı.");

        RuleFor(x => x.OriginalPrice).GreaterThan(0).WithMessage("Orijinal fiyat sıfırdan büyük olmalı.");

        // LessThan(x => x.OriginalPrice) → başka bir field'a göre karşılaştırma
        // FluentValidation'ın güçlü özelliklerinden biri: cross-field validation
        RuleFor(x => x.DiscountedPrice)
            .GreaterThan(0).WithMessage("İndirimli fiyat sıfırdan büyük olmalı.")
            .LessThan(x => x.OriginalPrice).WithMessage("İndirimli fiyat orijinal fiyattan düşük olmalı.");

        // GreaterThan(DateTime.UtcNow) → bitiş zamanı gelecekte olmalı
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Bitiş zamanı gelecekte olmalı.");
    }
}

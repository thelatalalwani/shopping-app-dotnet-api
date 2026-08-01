using ShoppingApp.Api.Interfaces;

namespace ShoppingApp.Api.Services;

public class DiscountCalculator
    : IDiscountCalculator
{
    public decimal ApplyFestivalDiscount(
        decimal amount)
    {
        const decimal discountPercentage = 10m;

        var discount =
            amount * discountPercentage / 100;

        return amount - discount;
    }

    public decimal ApplyBankDiscount(
        decimal amount)
    {
        const decimal discountPercentage = 5m;

        var discount =
            amount * discountPercentage / 100;

        return amount - discount;
    }
}

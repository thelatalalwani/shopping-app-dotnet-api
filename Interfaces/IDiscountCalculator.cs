namespace ShoppingApp.Api.Interfaces;

public interface IDiscountCalculator
{
    decimal ApplyFestivalDiscount(
        decimal amount);

    decimal ApplyBankDiscount(
        decimal amount);
}

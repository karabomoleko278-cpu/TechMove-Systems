namespace TechMoveSystems.Services;

public interface ICurrencyCalculator
{
    decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate);
}

public class CurrencyCalculator : ICurrencyCalculator
{
    public decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate)
    {
        if (usdAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "USD amount cannot be negative.");
        }

        if (exchangeRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero.");
        }

        return decimal.Round(usdAmount * exchangeRate, 2, MidpointRounding.AwayFromZero);
    }
}

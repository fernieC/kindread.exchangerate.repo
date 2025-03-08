using System.Threading.Tasks;

namespace KindRed.Exchange.Services
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertCurrencyAsync(decimal amount, string baseCurrency, string targetCurrency);
    }
}
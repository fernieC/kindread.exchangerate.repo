using System.Threading.Tasks;

namespace KindRed.Exchange.Services
{
    public interface ICurrencyCodeService
    {
        bool IsValidCurrencyCode(string currencyCode);
    }
}
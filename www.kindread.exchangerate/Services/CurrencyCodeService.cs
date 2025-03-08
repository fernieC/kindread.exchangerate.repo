
using KindRed.Exchange.Services;
namespace KindRed.Exchange.Services {

    public class CurrencyCodeService : ICurrencyCodeService
    {
        private readonly HashSet<string> ValidCurrencyCodes = new HashSet<string>
        {
            "AUD", "USD" // ISO 4217 codes
        };
        public CurrencyCodeService()
        {
            
        }
        public bool IsValidCurrencyCode(string currencyCode)
        {   
            var isCurrencyCodeOK = false;
            if( !string.IsNullOrEmpty(currencyCode)) {
                isCurrencyCodeOK = ValidCurrencyCodes.Contains(currencyCode.ToUpper());
            } 
            
            return isCurrencyCodeOK;
        }

    }
}
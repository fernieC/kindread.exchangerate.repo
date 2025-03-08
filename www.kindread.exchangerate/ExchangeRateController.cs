using KindRed.Exchange.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KindRed.Exchange.Controllers
{
    [Route("ExchangeService")]
    [ApiController]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ICurrencyService _currencyConverterService;
        private readonly ICurrencyCodeService _currencyCodeService;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController(ILogger<ExchangeRateController> logger,ICurrencyService currencyConverterService,ICurrencyCodeService currencyCodeService)
        {
            _currencyConverterService = currencyConverterService;
            _currencyCodeService = currencyCodeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertCurrency([FromBody] ConversionRequest request)
        {
            _logger.LogInformation($"a conversion request started at {DateTime.Now}");

            //performing some validation of the request
            if (request == null)
                return BadRequest("Request body cannot be null.");

            if (!_currencyCodeService.IsValidCurrencyCode(request.InputCurrency) || !_currencyCodeService.IsValidCurrencyCode(request.OutputCurrency)){
                return BadRequest("Either Input or output currency provided is not supported");
            }

            _logger.LogInformation($"conversion request: Amount: {request.Amount} - InputCurrency: {request.InputCurrency} - OutputCurrency: {request.OutputCurrency}");
            
            try
            {
                // inkoking the currency converter service
                var convertedValue = await _currencyConverterService.ConvertCurrencyAsync(request.Amount, request.InputCurrency, request.OutputCurrency);
                 
                var res = JsonConvert.SerializeObject(new ConversionResponse 
                                                            { Amount = request.Amount,
                                                              InputCurrency  = request.InputCurrency,
                                                              OutputCurrency = request.OutputCurrency,
                                                              Value = convertedValue
                                                            }); 
                return Content(res , "text/plain");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class ConversionRequest
    {
        public required decimal Amount { get; set; }
        public required string InputCurrency { get; set; }
        public required string OutputCurrency { get; set; }
    }
    public class ConversionResponse {
        public decimal Amount { get; set; }
        public string InputCurrency { get; set; }
        public string OutputCurrency { get; set; }
        public decimal Value { get; set; }  

    }
}

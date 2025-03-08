using Moq;
using KindRed.Exchange.Controllers;
using Microsoft.Extensions.Logging;
using KindRed.Exchange.Services;
//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;
//using Castle.Core.Logging;

namespace KindRed.Exchange.test;
 
public class ExchangeApiControllerTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void ConvertCurrency_ShouldReturnConvertedAmount()
    {
        // Arrange
        const string IN_CURRENCY  = "AUD";
        const string OUT_CURRENCY = "USD";
        const decimal IN_AMOUNT = 100m;
        const decimal OUT_AMOUNT = 65m;

        var request = new ConversionRequest { Amount = IN_AMOUNT, InputCurrency = IN_CURRENCY, OutputCurrency = OUT_CURRENCY };

        //Act
        var response = ConvertCurrency(request,OUT_AMOUNT,true);

     
        // Assert
        Assert.IsNotNull(response);

        var contentResult = (ContentResult)response.Result;
        var result = JsonConvert.DeserializeObject<ConversionResponse>(contentResult.Content);

        Assert.True(result.Value == OUT_AMOUNT);
        Assert.True(result.InputCurrency  == IN_CURRENCY);
        Assert.True(result.OutputCurrency == OUT_CURRENCY);
 
    }
    [Test]
    public void ConvertCurrency_ShouldReturnException()
    {
        // Arrange
        const string IN_CURRENCY = "AUD";
        const string OUT_CURRENCY = "XXX";
        const decimal IN_AMOUNT = 100;
        const decimal OUT_AMOUNT = 65m;

        var request = new ConversionRequest { Amount = IN_AMOUNT, InputCurrency = IN_CURRENCY, OutputCurrency = OUT_CURRENCY };

        //Act
        var response = ConvertCurrency(request, OUT_AMOUNT,false);

        // Assert
        Assert.IsNotNull(response);

        var contentResult = (BadRequestObjectResult)response.Result;
 
        Assert.True(contentResult.Value == "Either Input or output currency provided is not supported");

    }

    private Task<IActionResult> ConvertCurrency(ConversionRequest request,decimal expectedConvertedAmount,bool expectedCurrencyCodeValidation) 
    {

        // Set up the mock configuration for Logger
        var mockLogService = new Mock<ILogger<ExchangeRateController>>();

        //// Set up the mock configuration for CurrentService
        var mockCurrencyService = new Mock<ICurrencyService>();
        mockCurrencyService.Setup(s => s.ConvertCurrencyAsync(request.Amount, request.InputCurrency, request.OutputCurrency)).ReturnsAsync(expectedConvertedAmount);

        //// Set up the mock configuration for CurrencyCodeService
        var mockCurrencyCodeService = new Mock<ICurrencyCodeService>();
        var currencyCodes = new string[] { request.InputCurrency , request.OutputCurrency };
        mockCurrencyCodeService.Setup(s => s.IsValidCurrencyCode(It.IsIn<string>(currencyCodes))).Returns(expectedCurrencyCodeValidation);

        var controller = new ExchangeRateController(mockLogService.Object, mockCurrencyService.Object, mockCurrencyCodeService.Object);

        //// Act
        var response = controller.ConvertCurrency(request);

        return response;
    }
}
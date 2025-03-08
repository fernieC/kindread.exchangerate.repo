using System;
using System.Text.Json.Nodes;
using KindRed.Exchange.Controllers;
using KindRed.Exchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Moq.Protected;

namespace KindRed.Exchange.test;

public class ExchangeRateApiTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void ConvertCurrency_ShouldReturnConvertedAmount()
    {
        // Arrange
        const string IN_CURRENCY = "AUD";
        const string OUT_CURRENCY = "USD";
        const decimal IN_AMOUNT = 100m;
        const decimal OUT_AMOUNT = 65m;
        const decimal RATE = 0.65m;
        const string API_KEY = "c792c12c32f492d85080ea55";
        const string API_URL = "https://v6.exchangerate-api.com/v6/";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup the HttpClient to return a mock response
        mockHttpMessageHandler
            .Protected()  
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",  
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                //Content = new StringContent("{\"result\": \"success\", \"conversion_rates\": {\"USD\": 0.65}}")
                Content = new StringContent("{\"result\": \"success\", \"conversion_rates\": {" + OUT_CURRENCY + ": " + RATE + "}}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockCache = new Mock<IMemoryCache>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Set up the mock configuration to return predefined values for ApiKey and ApiUrl
        mockConfiguration.Setup(config => config.GetSection("ExchangeRateApi")["ApiKey"]).Returns(API_KEY);
        mockConfiguration.Setup(config => config.GetSection("ExchangeRateApi")["ApiUrl"]).Returns(API_URL);

        // Create the ExchangeRateApi instance with the mocked dependencies
        var exchangeRateApi = new ExchangeRateApi(httpClient, mockCache.Object, mockConfiguration.Object);

        // Mock the memory cache behavior
        decimal cachedRate = RATE; // example rate for conversion
        object objCacheRate = cachedRate;
        mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out objCacheRate)).Returns(true);  

        // Act
        var result = exchangeRateApi.ConvertCurrencyAsync(IN_AMOUNT, IN_CURRENCY, OUT_CURRENCY).Result;

        // Assert
        
        Assert.AreEqual(OUT_AMOUNT, result); 

    }
}

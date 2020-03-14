using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PaymentGateway.Providers;
using Xunit;

namespace PaymentGateway.UnitTests.Providers
{
    public class HscbAcquirerTests
    {
        private HttpClient _httpClient;
        private IHttpClientFactory _httpClientFactory;
        private HsbcAquirerBank _hsbcAcquirer;

        public HscbAcquirerTests()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _hsbcAcquirer = new HsbcAquirerBank(Substitute.For<ILogger<HsbcAquirerBank>>(), _httpClientFactory);
        }

        [Fact]
        public async Task GivenSuccessfulResponse_WhenProcessPaymentIsCalled_ThenSuccessPaymentResponseIsReturned()
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler("{\"resultCode\":\"OK\"}", HttpStatusCode.OK)) { BaseAddress = new Uri(@"http://ui.com") };

            _httpClientFactory.CreateClient(Application.Constants.HsbcBankhttpClientName)
                .Returns(httpClient);

            var response = await _hsbcAcquirer.ProcessPayment(new PaymentGateway.Domain.Interfaces.AcquiringBank.ProcessPaymentRequest());

            response.IsSuccess.Should().BeTrue(); 
        }

        [Fact]
        public async Task GivenUnsuccessfulResponse_WhenProcessPaymentIsCalled_ThenUnsuccessPaymentResponseIsReturned()
        {
            _httpClient = new HttpClient(new MockHttpMessageHandler("{\"resultCode\":\"Error\",\"resultDescription\":\"Error desc\" }", HttpStatusCode.BadRequest)) { BaseAddress = new Uri(@"http://ui.com") };

            _httpClientFactory.CreateClient(Application.Constants.HsbcBankhttpClientName)
                .Returns(_httpClient);

            var response = await _hsbcAcquirer.ProcessPayment(new PaymentGateway.Domain.Interfaces.AcquiringBank.ProcessPaymentRequest());

            response.IsSuccess.Should().BeFalse();
            response.ResultCode.Should().Be("Error");         
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _statusCode;
        

        public string Input { get; private set; }
        public int NumberOfCalls { get; private set; }

        public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
        {
            _response = response;
            _statusCode = statusCode;            
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            NumberOfCalls++;
            Input = await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_response)
            };
        }
    }
}

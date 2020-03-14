using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure;
using Xunit;

namespace PaymentGateway.IntegrationTests
{
    public class ProcessPaymentTests : IntegrationTestBase, IAsyncLifetime
    {

        private string enpointUrl = "/api/v1/payments";
        private IPaymentRepository _paymentRepository;
        private Merchant _testMerchant;

        public async Task InitializeAsync()
        {
            _paymentRepository = ResolveService<IPaymentRepository>();

            var merchantRepository = ResolveService<IMerchantRepository>();
            _testMerchant = BuildTestMerchant();
            await merchantRepository.AddMerchant(_testMerchant);

            HttpClient.DefaultRequestHeaders.Add(Application.Constants.ApiKeyHeaderName, _testMerchant.ApiKey);
        }

        [Fact]
        public async Task GivenValidNewPayment_WhenProcessPaymentIsCalled_ThenPaymentIsCreatedWithSuccessStatus()
        {
            var validRequest = BuildValidRequest();

            await PostAsJsonAsync(enpointUrl, validRequest);

            var payment = await _paymentRepository.GetByMerchantTransactionId(_testMerchant.Id, validRequest.MerchantTransactionId);

            payment.Should().NotBeNull();
            payment.GetPaymentStatus().Status.Should().Be(PaymentStatusEnum.FinishedSuccesfully);
        }

        [Fact]
        public async Task GivenInvalidApiKey_WhenProcessPaymentIsCalled_ThenUnauthorizedIsReturned()
        {
            var validRequest = BuildValidRequest();
            HttpClient.DefaultRequestHeaders.Remove(Application.Constants.ApiKeyHeaderName);
            HttpClient.DefaultRequestHeaders.Add(Application.Constants.ApiKeyHeaderName,"fake key");

            var httpResponse = await PostAsJsonAsync(enpointUrl, validRequest);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GivenInvaliSignature_WhenProcessPaymentIsCalled_ThenBadRequestIsReturned()
        {
            var validRequest = BuildValidRequest();
            validRequest.Signature = "Invalid signature";

            var httpResponse = await PostAsJsonAsync(enpointUrl, validRequest);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private ProcessPaymentRequest BuildValidRequest()
        {
            var request = new ProcessPaymentRequest
            {
                MerchantTransactionId = Guid.NewGuid().ToString(),
                Amount = 10,
                CardNumber = "1234123412341234",
                ExpiryMonth = 10,
                ExpiryYear = 2022,
                Cvv = "123",
                Currency = "EUR"               
            };

            request.Signature = CalculateSignature(request);

            return request;
        }

        private string CalculateSignature(ProcessPaymentRequest request)
        {
            var parametrs = new Dictionary<string, string>
            {
                {nameof(request.MerchantTransactionId), request.MerchantTransactionId},
                {nameof(request.Amount), request.Amount.ToString("F",CultureInfo.InvariantCulture)},
                {nameof(request.CardNumber), request.CardNumber},
                {nameof(request.ExpiryMonth), request.ExpiryMonth.ToString()},
                {nameof(request.ExpiryYear), request.ExpiryYear.ToString()},
                {nameof(request.Cvv), request.Cvv},
                {nameof(request.Currency), request.Currency}
            };

            var hmacGenerator = new HmacValidator();

            return hmacGenerator.CalculateSignature(hmacGenerator.BuildSigningString(parametrs), _testMerchant.SigningKey);
        }

        private Merchant BuildTestMerchant()
        {
            return new Merchant
            {
                ApiKey = Guid.NewGuid().ToString(),
                Code = "testMerchant",
                SigningKey = Guid.NewGuid().ToString()
            };
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

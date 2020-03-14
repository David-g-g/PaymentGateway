using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;
using Xunit;

namespace PaymentGateway.IntegrationTests
{
    public class GetPaymentTests : IntegrationTestBase, IAsyncLifetime
    {

        private string enpointUrl = "/api/v1/payments/{0}";
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
        public async Task GivenExistingPayment_WhenGetPaymentIsCalled_ThenPaymentReturned()
        {
            var existingPayment = BuildPayment();
            await _paymentRepository.AddPayment(existingPayment);
            var url = string.Format(enpointUrl, existingPayment.Id.ToString());

            var httpResponse = await HttpClient.GetAsync(url);

            var payment = ReadAsJsonAsync<Payment>(httpResponse.Content);

            payment.Should().NotBeNull();
        }

        private Payment BuildPayment()
        {
            return new Payment
            {
                MerchantTransactionId = Guid.NewGuid().ToString(),
                Amount = 10,
                CardDetails = new Card
                {
                    CardNumber = "1234123412341234",
                    ExpiryMonth = 10,
                    ExpiryYear = 2022,
                    Cvv = "123",
                },                
                Currency = "EUR",
                AcquirerResponse = new AcquirerResponse
                {
                    IsSuccess = true,
                    TransactionId = Guid.NewGuid().ToString(),
                    ResultCode = "OK"
                }

            };
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

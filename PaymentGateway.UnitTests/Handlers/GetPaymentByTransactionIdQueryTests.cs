using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using PaymentGateway.Application.Queries.GetPaymentByTransactionId;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;
using Xunit;

namespace PaymentGateway.UnitTests.Handlers
{
    public class GetPaymentByTransactionIdQueryHandlerTests
    {
        private IMerchantRepository _merchantRepository;
        private IPaymentRepository _paymentrepository;
        private GetPaymentByTransactionIdQueryHandler _handler;

        public GetPaymentByTransactionIdQueryHandlerTests()
        {
            _merchantRepository = Substitute.For<IMerchantRepository>();
            _paymentrepository = Substitute.For<IPaymentRepository>();

            _handler = new GetPaymentByTransactionIdQueryHandler(_merchantRepository, _paymentrepository);
        }

        [Fact]
        public async Task GivenExistingPayment_WhenExecuteIsCalled_ThenPaymentIsReturnedAsExpected()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId };
            var merchantTransactionId = "merchant tranid";
            var transactionId = Guid.NewGuid();
            var amount = 15.4m;
            var currency = "EUR";
            var cardNumber = "1234123412341234";
            var expiryMonth = 12;
            var expiryYear = 2020;
            var cvv = "123";
            var acquirerResultCode = "ok";
            var acquirerResultDescription = "Result descroption";
            var acquirerTransactionId = "transciontionId";
            var query = new GetPaymentByTransactionIdQuery { ApiKey = apiKey, TransactionId = transactionId };
            var payment = new Payment
            {
                MerchantTransactionId = merchantTransactionId,
                TransactionId = transactionId,
                Amount = amount,
                Currency = currency,
                CardDetails = new Card
                {
                    CardNumber = cardNumber,
                    ExpiryMonth = expiryMonth,
                    ExpiryYear = expiryYear,
                    Cvv = cvv
                },
                AcquirerResponse = new AcquirerResponse
                {
                    TransactionId = acquirerTransactionId,
                    IsSuccess = true,
                    ResultCode = acquirerResultCode,
                    ResultDescription = acquirerResultDescription
                }
            };

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _paymentrepository.GetByTransactionId(merchant.Id, transactionId).Returns(payment);


            var response = await _handler.Handle(query, new System.Threading.CancellationToken());

            response.Errors.Should().BeEmpty();

            response.Value.Should().BeEquivalentTo(new GetPaymentByTransactionIdQueryResponse
            {
                TransactionId = transactionId,
                Amount = amount,
                Currency = currency,
                CardNumber = "****1234",
                ExpiryYear = expiryYear,
                ExpiryMonth = expiryMonth,
                Cvv = cvv,
                StatusCode = acquirerResultCode
            });
        }

        [Fact]
        public async Task GivenNotExistingPayment_WhenExecuteIsCalled_ThenNullIsReturned()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId };
            var transactionId = Guid.NewGuid();
            var query = new GetPaymentByTransactionIdQuery { ApiKey = apiKey, TransactionId = transactionId };

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);

            var response = await _handler.Handle(query, new System.Threading.CancellationToken());

            response.Errors.Should().BeEmpty();

            response.Value.Should().BeNull();
        }
    }
}

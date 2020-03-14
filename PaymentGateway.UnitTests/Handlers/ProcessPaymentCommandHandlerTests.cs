using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Domain.Interfaces.AcquiringBank;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;
using Xunit;

namespace PaymentGateway.UnitTests.Handlers
{
    public class RequestPaymentCommandHandlerTests
    {
        private IMerchantRepository _merchantRepository;
        private IPaymentRepository _paymentrepository;
        private IAcquiringBank _acquiringBank;
        private ILogger<RequestPaymentCommandHandler> _logger;
        private RequestPaymentCommandHandler _handler;

        public RequestPaymentCommandHandlerTests()
        {
            _merchantRepository = Substitute.For<IMerchantRepository>();
            _paymentrepository = Substitute.For<IPaymentRepository>();
            _acquiringBank = Substitute.For<IAcquiringBank>();
            _logger = Substitute.For<ILogger<RequestPaymentCommandHandler>>();

            _handler = new RequestPaymentCommandHandler(_logger, _merchantRepository, _paymentrepository, _acquiringBank);
        }

        [Fact]
        public async Task GivenValidPaymentRequest_WhenExecuteIsCalled_ThenPaymentIsCreated()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId };
            var merchantTransactionId = "tranid";
            var amount = 15.4m;
            var currency = "EUR";
            var cardNumber = "1234";
            var expiryMonth = 12;
            var expityYear = 2020;
            var cvv = "123";
            var acquirerResultCode = "ok";
            var acquirerResultDescription = "Result descroption";
            var acquirerTransactionId = "transciontionId";

            var requestPaymentCommand = new RequestPaymentCommand
            {
                MerchantTransactionId = merchantTransactionId,
                ApiKey = apiKey,
                Amount = amount,
                Currency = currency,
                CardNumber = cardNumber,
                ExpiryMonth = expiryMonth,
                ExpiryYear = expityYear,
                Cvv = cvv
            };

            var acquirerSuccessResponse = new ProcessPaymentResponse { IsSuccess = true, ResultCode = acquirerResultCode, ResultDescription = acquirerResultDescription, TransactionId = acquirerTransactionId };
                                   
            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);

            _acquiringBank.ProcessPayment(Arg.Is<ProcessPaymentRequest>(
                p => p.Amount == amount
                    && p.Currency == currency
                    && p.CardNumber == cardNumber
                    && p.ExpiryMonth == expiryMonth
                    && p.ExpiryYear == expityYear
                    && p.Cvv == cvv))
               .Returns(acquirerSuccessResponse);


            await _handler.Handle(requestPaymentCommand, new System.Threading.CancellationToken());


            await _paymentrepository.Received(1).AddPayment(Arg.Is<Payment>(p => p.MerchantId.Equals(merchantId)
                                                                && p.Amount == amount
                                                                && p.Currency == currency
                                                                && p.CardDetails.CardNumber == cardNumber
                                                                && p.CardDetails.ExpiryMonth == expiryMonth
                                                                && p.CardDetails.ExpiryYear == expityYear
                                                                && p.CardDetails.Cvv == cvv));

            await _paymentrepository.Received(1).UpdatePayment(Arg.Is<Payment>(p => p.MerchantId.Equals(merchantId)
                                                                && p.Amount == amount
                                                                && p.Currency == currency
                                                                && p.CardDetails.CardNumber == cardNumber
                                                                && p.CardDetails.ExpiryMonth == expiryMonth
                                                                && p.CardDetails.ExpiryYear == expityYear
                                                                && p.CardDetails.Cvv == cvv
                                                                && p.AcquirerResponse.IsSuccess == true
                                                                && p.AcquirerResponse.ResultCode == acquirerResultCode
                                                                && p.AcquirerResponse.ResultDescription == acquirerResultDescription
                                                                && p.AcquirerResponse.TransactionId == acquirerTransactionId));




        }

        [Fact]
        public async Task GivenConcurrencyException_WhenExecuteIsCalled_ThenPaymentIsNotUpdated()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId };
            var merchantTransactionId = "tranid";
            var amount = 15.4m;
            var currency = "EUR";
            var cardNumber = "1234";
            var expiryMonth = 12;
            var expityYear = 2020;
            var cvv = "123";
            var acquirerResultCode = "ok";
            var acquirerResultDescription = "Result descroption";
            var acquirerTransactionId = "transciontionId";

            var requestPaymentCommand = new RequestPaymentCommand
            {
                MerchantTransactionId = merchantTransactionId,
                ApiKey = apiKey,
                Amount = amount,
                Currency = currency,
                CardNumber = cardNumber,
                ExpiryMonth = expiryMonth,
                ExpiryYear = expityYear,
                Cvv = cvv
            };

            var acquirerSuccessResponse = new ProcessPaymentResponse { IsSuccess = true, ResultCode = acquirerResultCode, ResultDescription = acquirerResultDescription, TransactionId = acquirerTransactionId };

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);

            _paymentrepository.AddPayment(Arg.Is<Payment>(
                p => p.MerchantId.Equals(merchantId)
                    && p.Amount == amount
                    && p.Currency == currency
                    && p.CardDetails.CardNumber == cardNumber
                    && p.CardDetails.ExpiryMonth == expiryMonth
                    && p.CardDetails.ExpiryYear == expityYear
                    && p.CardDetails.Cvv == cvv))
                .Throws(new ArgumentException());


            await _handler.Handle(requestPaymentCommand, new System.Threading.CancellationToken());
                        

            await _paymentrepository.Received(0).UpdatePayment(Arg.Any<Payment>());
            await _acquiringBank.Received(0).ProcessPayment(Arg.Any<ProcessPaymentRequest>());

        }
    }
}

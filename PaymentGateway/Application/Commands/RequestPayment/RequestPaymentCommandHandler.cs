using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Interfaces.AcquiringBank;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Commands.RequestPayment
{
    public class RequestPaymentCommandHandler : IRequestHandler<RequestPaymentCommand, Result>
    {
        private readonly ILogger _logger;
        private readonly IMerchantRepository _merchantRepository;   
        private readonly IPaymentRepository _paymentRequestRepository;
        private readonly IAcquiringBank _acquiringBank;

        public RequestPaymentCommandHandler(ILogger<RequestPaymentCommandHandler> logger,
                                            IMerchantRepository merchantRepository,
                                            IPaymentRepository paymentRequestRepository,
                                            IAcquiringBank acquiringBank)
        {
            _logger = logger;
            _merchantRepository = merchantRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _acquiringBank = acquiringBank;
        }

        public async Task<Result> Handle(RequestPaymentCommand command, CancellationToken cancellationToken)
        {
            var merchant = await _merchantRepository.GetByApiKey(command.ApiKey);

            var payment = BuildPayment(merchant, command);

            try
            {
                await _paymentRequestRepository.AddPayment(payment);
            }
            catch (ArgumentException ex)
            {
                //Concurrency exception, transaction already processed (or in progress) for that merchant
                _logger.LogInformation($"Concurrency exception registering payment for Transaction: {command.MerchantTransactionId}, Merchant: {merchant.Code}. ex: {ex}");

                return Result.Empty();
            }           

            var acquirerResponse = await _acquiringBank.ProcessPayment(BuildAcquirerRequest(command));

            payment.RegisterAcquirerResponse(acquirerResponse);
            
            await _paymentRequestRepository.UpdatePayment(payment);

            return Result.Empty();
        }

        private ProcessPaymentRequest BuildAcquirerRequest(RequestPaymentCommand command)
        {
            return new ProcessPaymentRequest
            {
                Amount = command.Amount,
                Currency = command.Currency,
                CardNumber = command.CardNumber,
                ExpiryYear = command.ExpiryYear,
                ExpiryMonth = command.ExpiryMonth,
                Cvv = command.Cvv
            };
        }

        private static Payment BuildPayment(Merchant merchant, RequestPaymentCommand command)
        {
            var paymentRequest = new Payment
            {
                TransactionId = Guid.NewGuid(),
                MerchantId = merchant.Id,
                MerchantTransactionId = command.MerchantTransactionId,
                Amount = command.Amount,
                Currency = command.Currency,
                CardDetails = new Card { CardNumber = command.CardNumber,
                                        ExpiryMonth = command.ExpiryMonth,
                                        ExpiryYear = command.ExpiryYear,
                                        Cvv = command.Cvv }
            };

            return paymentRequest;
        }       
    }
}
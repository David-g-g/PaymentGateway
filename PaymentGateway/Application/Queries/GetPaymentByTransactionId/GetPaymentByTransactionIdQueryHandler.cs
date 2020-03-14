using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PaymentGateway.Domain.Interfaces.Repository;

namespace PaymentGateway.Application.Queries.GetPaymentByTransactionId
{
    public class GetPaymentByTransactionIdQueryHandler : IRequestHandler<GetPaymentByTransactionIdQuery, Result<GetPaymentByTransactionIdQueryResponse>>
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByTransactionIdQueryHandler(IMerchantRepository merchantRepository, IPaymentRepository paymentRequestRepository)
        {
            _merchantRepository = merchantRepository;
            _paymentRepository = paymentRequestRepository;
        }

        public async Task<Result<GetPaymentByTransactionIdQueryResponse>> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
        {
            var merchant = await _merchantRepository.GetByApiKey(request.ApiKey);

            var payment = await _paymentRepository.GetByTransactionId(merchant.Id, request.TransactionId);

            if (payment == null)
            {
                return new Result<GetPaymentByTransactionIdQueryResponse>();
            }

            return new Result<GetPaymentByTransactionIdQueryResponse>
            {
                Value = new GetPaymentByTransactionIdQueryResponse
                {
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    CardNumber = $"****{payment.CardDetails.CardNumber.Substring(payment.CardDetails.CardNumber.Length - 4)}",
                    ExpiryYear = payment.CardDetails.ExpiryYear,
                    ExpiryMonth = payment.CardDetails.ExpiryMonth,
                    StatusCode = payment.GetPaymentStatus().StatusCode,
                    TransactionId = payment.TransactionId,
                    Cvv = payment.CardDetails.Cvv
                }
            };
        }
    }
}

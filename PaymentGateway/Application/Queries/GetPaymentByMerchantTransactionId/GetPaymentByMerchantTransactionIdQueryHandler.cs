using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PaymentGateway.Domain.Interfaces.Repository;

namespace PaymentGateway.Application.Queries
{
    public class GetPaymentByMerchantTransactionIdQueryHandler : IRequestHandler<GetPaymentByMerchantTransactionIdQuery, Result<GetPaymentByMerchantTransactionIdQueryResponse>>
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByMerchantTransactionIdQueryHandler(IMerchantRepository merchantRepository, IPaymentRepository paymentRepository)
        {
            _merchantRepository = merchantRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<GetPaymentByMerchantTransactionIdQueryResponse>> Handle(GetPaymentByMerchantTransactionIdQuery request, CancellationToken cancellationToken)
        {
            var merchant = await _merchantRepository.GetByApiKey(request.ApiKey);

            var payment = await _paymentRepository.GetByMerchantTransactionId(merchant.Id, request.MerchantTransactionId);

            if (payment == null)
            {
                return null;
            }

            var transactionStatus = payment.GetPaymentStatus();

            return new Result<GetPaymentByMerchantTransactionIdQueryResponse>
            {
                Value = new GetPaymentByMerchantTransactionIdQueryResponse
                {
                    TransactionId = payment.TransactionId,
                    IsSuccessfull = transactionStatus.Status == Domain.Models.PaymentStatusEnum.FinishedSuccesfully,
                    ResultCode = transactionStatus.StatusCode,
                    ResultDescription = transactionStatus.StatusReason
                }
            };
        }
    }
}

using System;
using MediatR;

namespace PaymentGateway.Application.Queries
{
    public class GetPaymentByMerchantTransactionIdQuery:IRequest<Result<GetPaymentByMerchantTransactionIdQueryResponse>>
    {
        public string ApiKey { get; set; }
        public string MerchantTransactionId { get; set; }
    }
}

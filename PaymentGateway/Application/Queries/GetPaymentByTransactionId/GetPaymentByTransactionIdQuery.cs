using System;
using MediatR;

namespace PaymentGateway.Application.Queries.GetPaymentByTransactionId
{
    public class GetPaymentByTransactionIdQuery:IRequest<Result<GetPaymentByTransactionIdQueryResponse>>
    {
        public string ApiKey { get; set; }
        public Guid TransactionId { get; set; }
    }
}

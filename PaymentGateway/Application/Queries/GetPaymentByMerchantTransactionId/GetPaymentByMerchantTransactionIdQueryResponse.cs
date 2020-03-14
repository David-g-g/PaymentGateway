using System;
namespace PaymentGateway.Application.Queries
{
    public class GetPaymentByMerchantTransactionIdQueryResponse
    {
        public bool IsSuccessfull { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public Guid TransactionId { get; set; }
    }
}

using System;
namespace PaymentGateway.Application.Queries.GetPaymentByTransactionId
{
    public class GetPaymentByTransactionIdQueryResponse
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public string CreatedDate { get; set; }
        public string StatusCode { get; set; }
    }
}

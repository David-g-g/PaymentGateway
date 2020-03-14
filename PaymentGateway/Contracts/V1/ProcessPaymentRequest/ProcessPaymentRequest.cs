using System;
namespace PaymentGateway.Contracts.V1.ProcessPaymentRequest
{
    public class ProcessPaymentRequest
    {
        public string MerchantTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public string Signature { get; set; }
    }
}

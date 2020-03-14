using System;
namespace PaymentGateway.Contracts.V1.GetPayment
{
    public class GetPaymentResponse
    {
        public Guid transactionId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryDay { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public string CreatedDate { get; set; }
        public string StatusCode { get; internal set; }
    }
}

namespace PaymentGateway.Domain.Interfaces.AcquiringBank
{
    public class ProcessPaymentRequest
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
    }
}
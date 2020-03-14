namespace PaymentGateway.Providers
{
    public class HsbcProcessPaymentRequest
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryDay { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
    }

    public class HsbcProcessPaymentResponse
    {
        public string ResultCode { get; set; }
        public string TransactionId { get; set; }
    }
}
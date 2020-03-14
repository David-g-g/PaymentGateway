namespace PaymentGateway.Domain.Interfaces.AcquiringBank
{
    public class ProcessPaymentResponse
    {
        public string TransactionId { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public bool IsSuccess { get; set; }
    }
}
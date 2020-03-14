using System;

namespace PaymentGateway.Domain.Models
{
    public class AcquirerResponse
    {
        public AcquirerResponse()
        {
            CreatedDate = DateTime.UtcNow;
        }
        public string TransactionId { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
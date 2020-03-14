using System;
using System.Collections.Generic;

namespace PaymentGateway.Contracts.V1.ProcessPaymentRequest
{
    public class ProcessPaymentResponse
    {
        public Guid TransactionId { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
    }
}

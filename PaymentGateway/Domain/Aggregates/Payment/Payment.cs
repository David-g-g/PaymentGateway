using System;
using PaymentGateway.Domain.Interfaces.AcquiringBank;

namespace PaymentGateway.Domain.Models
{
    public class Payment
    {
        public Payment()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string MerchantTransactionId { get; set; }
        public decimal Amount { get; set; }
        public Guid MerchantId { get; set; }
        public Card CardDetails { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        public AcquirerResponse AcquirerResponse { get; set; }
        public Guid TransactionId { get; set; }

        public void RegisterAcquirerResponse(ProcessPaymentResponse acquirerResponse)
        {
            AcquirerResponse = new AcquirerResponse
            {
                ResultCode = acquirerResponse.ResultCode,
                ResultDescription = acquirerResponse.ResultDescription,
                TransactionId = acquirerResponse.TransactionId,
                IsSuccess = acquirerResponse.IsSuccess
            };
        }

        public PaymentStatus GetPaymentStatus()
        {
            if (AcquirerResponse == null)
            {
                return new PaymentStatus
                {
                    Status = PaymentStatusEnum.Processing,
                    StatusCode = "P1",
                    StatusReason = $"Transaction in progress. Started {CreatedDate}"
                };
            }

            return new PaymentStatus
            {
                Status = AcquirerResponse.IsSuccess ? PaymentStatusEnum.FinishedSuccesfully : PaymentStatusEnum.FinishedFailed,
                StatusCode = AcquirerResponse.ResultCode,
                StatusReason = AcquirerResponse.IsSuccess ?
                               $"Payment processed on {AcquirerResponse.CreatedDate}"
                               : AcquirerResponse.ResultDescription
            };
        }

    }

    public class PaymentStatus
    {
        public PaymentStatusEnum Status { get; set; }
        public string StatusCode { get; set; }
        public string StatusReason { get; set; }
    }

    public enum PaymentStatusEnum
    {
        Processing,
        FinishedSuccesfully,
        FinishedFailed
    }
}

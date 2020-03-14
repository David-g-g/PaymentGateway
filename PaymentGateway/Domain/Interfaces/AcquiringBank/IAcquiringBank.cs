using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Interfaces.AcquiringBank
{
    public interface IAcquiringBank
    {
        Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request);
    }
}

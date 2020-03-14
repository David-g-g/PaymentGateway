using System;
using System.Threading.Tasks;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Interfaces.Repository
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByMerchantTransactionId(Guid merchantId, string id);
        Task<Payment> GetByTransactionId(Guid merchantId, Guid transasctionId);
        Task AddPayment(Payment paymentRequest);
        Task UpdatePayment(Payment paymentRequest);
    }
}

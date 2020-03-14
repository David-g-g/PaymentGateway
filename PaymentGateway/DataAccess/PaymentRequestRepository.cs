using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.DataAccess
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDictionary<string, Payment> PaymentRequests = BuildTestPayment();

        public Task AddPayment(Payment paymentRequest)
        {
            var key = FormatKey(paymentRequest.MerchantId, paymentRequest.MerchantTransactionId);
            PaymentRequests.Add(key, paymentRequest);

            return Task.CompletedTask;
        }

        public Task<Payment> GetByMerchantTransactionId(Guid merchantId, string transactionId)
        {
            var key = FormatKey(merchantId, transactionId);
            return Task.FromResult(PaymentRequests.ContainsKey(key) ? PaymentRequests[key] : null);
        }

        public Task<Payment> GetByTransactionId(Guid merchantId, Guid transactionId)
        {
            var payment = PaymentRequests.Values.FirstOrDefault(payment => payment.TransactionId.Equals(transactionId) && payment.MerchantId.Equals(merchantId));

            return Task.FromResult(payment);
        }

        public Task UpdatePayment(Payment paymentRequest)
        {
            var key = FormatKey(paymentRequest.MerchantId, paymentRequest.MerchantTransactionId);
            PaymentRequests[key] = paymentRequest;

            return Task.CompletedTask;
        }

        private string FormatKey(Guid merchantId, string transactionId)
        {
            return $"{merchantId}-{transactionId}";
        }

        private static Dictionary<string, Payment> BuildTestPayment()
        {
            return new Dictionary<string, Payment>
            {
                { $"{"7fb6f154-9869-4917-8be4-f0767b12cd37"}-{"1234567890"}",
                    new Payment {
                    TransactionId = new Guid("7fb6f154-9869-4917-8be4-f0767b12cd37"),
                    MerchantId = new Guid("7fb6f154-9869-4917-8be4-f0767b12cd37"),
                    Amount = 122,
                    Currency="EUR",
                    MerchantTransactionId = "1234567890",
                    CardDetails = new Card{CardNumber = "00011221212112", Cvv = "123", ExpiryYear=12, ExpiryMonth = 34},
                    AcquirerResponse = new AcquirerResponse{ IsSuccess  =true, CreatedDate = DateTime.UtcNow, ResultCode = "S100", ResultDescription = "Processed OK"} } }
            };
        }
    }
}

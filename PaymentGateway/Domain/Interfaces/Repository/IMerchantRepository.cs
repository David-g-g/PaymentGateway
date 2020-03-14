using System;
using System.Threading.Tasks;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Interfaces.Repository
{
    public interface IMerchantRepository
    {
        Task<Merchant> GetByApiKey(string apiKey);
        Task AddMerchant(Merchant testMerchant);
    }
}

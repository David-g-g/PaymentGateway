using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.DataAccess
{
    public class CachedMerchantRepository : IMerchantRepository
    {
        private IList<Merchant> _merchants = new List<Merchant> { BuildTestMerchant() };
               
        public Task<Merchant> GetByApiKey(string apiKey)
        {
            return Task.FromResult(_merchants.FirstOrDefault(m => m.ApiKey == apiKey));
        }

        public Task AddMerchant(Merchant Merchant)
        {
            _merchants.Add(Merchant);

            return Task.CompletedTask;
        }

        private static Merchant BuildTestMerchant()
        {
            return new Merchant
            {
                Id = new Guid("7fb6f154-9869-4917-8be4-f0767b12cd37"),
                ApiKey = "testkey",
                Code = "TestMerchant",
                SigningKey = "DFB1EB5485895CFA84146406857104ABB4CBCABDC8AAF103A624C8F6A3EAAB00"
            };
        }

    }
}

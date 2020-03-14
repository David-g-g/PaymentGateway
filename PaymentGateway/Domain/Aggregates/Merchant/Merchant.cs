using System;
namespace PaymentGateway.Domain.Models
{
    public class Merchant
    {
        public Merchant()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string ApiKey { get; set; }
        public string SigningKey { get; set; }
    }
}
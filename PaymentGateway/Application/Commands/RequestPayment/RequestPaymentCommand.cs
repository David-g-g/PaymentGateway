using System.Collections.Generic;
using System.Globalization;
using MediatR;
namespace PaymentGateway.Application.Commands.RequestPayment
{
    public class RequestPaymentCommand : IRequest<Result>
    {
        public string ApiKey { get; set; }
        public string MerchantTransactionId { get; set; }
        public decimal  Amount {get; set;}
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public string Signature { get; set; }

        internal IDictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {                
                {nameof(MerchantTransactionId), MerchantTransactionId},
                {nameof(Amount), Amount.ToString("F",CultureInfo.InvariantCulture)},
                {nameof(CardNumber), CardNumber},
                {nameof(ExpiryMonth), ExpiryMonth.ToString()},
                {nameof(ExpiryYear), ExpiryYear.ToString()},
                {nameof(Cvv), Cvv},
                {nameof(Currency), Currency}
            };
        }
    }
}
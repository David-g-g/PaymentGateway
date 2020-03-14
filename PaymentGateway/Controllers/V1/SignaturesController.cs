using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("api/v1/signatures")]
    public class SignaturesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GenerateSignature([FromQuery] GenerateSignatureRequest request)
        {
            var hmacValidator = new HmacValidator();

            var stringToSign = hmacValidator.BuildSigningString(GetDictonaryFromRequest(request));
            var signature = hmacValidator.CalculateSignature(stringToSign, request.SigningKey);

            return Ok(signature);
        }

        private Dictionary<string, string>  GetDictonaryFromRequest(GenerateSignatureRequest request)
        {
                return new Dictionary<string, string>
            {
                {nameof(request.MerchantTransactionId), request.MerchantTransactionId},
                {nameof(request.Amount), request.Amount.ToString("F",CultureInfo.InvariantCulture)},
                {nameof(request.CardNumber), request.CardNumber},
                {nameof(request.ExpiryMonth), request.ExpiryMonth.ToString()},
                {nameof(request.ExpiryYear), request.ExpiryYear.ToString()},
                {nameof(request.Cvv), request.Cvv},
                {nameof(request.Currency), request.Currency}
            };

        }
    }

    public class GenerateSignatureRequest
    {
        public string SigningKey { get; set; }
        public string MerchantTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
    }
}

using System.Collections.Generic;

namespace PaymentGateway.Domain
{
    public interface ISignatureValidator
    {
        bool IsValidSignature(IDictionary<string, string> messageFields, string originalSignature, string key);
    };
}
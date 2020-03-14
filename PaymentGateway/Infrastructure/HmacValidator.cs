using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class HmacValidator : ISignatureValidator
    {
        public string CalculateSignature(string signingstring, string hmacKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(hmacKey);
            byte[] data = Encoding.UTF8.GetBytes(signingstring);

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] rawHmac = hmac.ComputeHash(data);

                return Convert.ToBase64String(rawHmac);
            }
        }


        public bool IsValidSignature(IDictionary<string, string> messageFields, string originalSignature, string key)
        {
            string stringToSign = BuildSigningString(messageFields);
            string expectedSign = CalculateSignature(stringToSign, key);
            return string.Equals(expectedSign, originalSignature);
        }

        public string BuildSigningString(IDictionary<string, string> dict)
        {
            var signDict = dict.OrderBy(d => d.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

            string keystring = string.Join(":", signDict.Keys);
            string valuestring = string.Join(":", signDict.Values.Select(EscapeVal));

            return string.Format("{0}:{1}", keystring, valuestring);
        }

        private string EscapeVal(string val)
        {
            if (val == null)
            {
                return string.Empty;
            }

            val = val.Replace(@"\", @"\\");
            val = val.Replace(":", @"\:");
            return val;
        }
    }
}

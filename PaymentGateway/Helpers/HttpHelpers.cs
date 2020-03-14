using Microsoft.AspNetCore.Http;
using PaymentGateway.Application;

namespace PaymentGateway.Helpers
{
    public static class HttpHelper
    {       
        public static string GetMerchantApiKey(this HttpRequest request)
        {
           if (request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out var apiKey ))
           {
              return apiKey;
           } 

           return string.Empty;
        }
    }
}
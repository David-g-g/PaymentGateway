using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application;
using PaymentGateway.Domain.Interfaces.AcquiringBank;

namespace PaymentGateway.Providers
{
    public class HsbcAquirerBank : IAcquiringBank
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HsbcAquirerBank> _logger;

        public HsbcAquirerBank(ILogger<HsbcAquirerBank> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            try
            {
                var processPaymentUrl = string.Empty; //Get from config

                using var httpclient = _httpClientFactory.CreateClient(Constants.HsbcBankhttpClientName);

                var httpResponse = await httpclient.PostAsync(processPaymentUrl, BuildRequest());

                httpResponse.EnsureSuccessStatusCode();

                return await BuildResponse(httpResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error contacting with HscbAcquirer", ex);

                return new ProcessPaymentResponse
                {
                    IsSuccess = false,
                    ResultCode = "Error", // TODO:map error code if returned
                    ResultDescription = ex.Message // TODO:extact content from response
                };
            }
        }

        private async Task<ProcessPaymentResponse> BuildResponse(HttpResponseMessage httpResponse)
        {
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();

            var hsbcResponse = JsonSerializer.Deserialize<HsbcProcessPaymentResponse>(stringResponse);

            return new ProcessPaymentResponse
            {
                IsSuccess = true,
                ResultCode = "OK", //map to result codes
                TransactionId = hsbcResponse.TransactionId
            };
        }

        private static StringContent BuildRequest()
        {
            var bankRequest = new HsbcProcessPaymentRequest { };

            var content = new StringContent(JsonSerializer.Serialize(bankRequest), System.Text.Encoding.UTF8, "application/json");
            return content;
        }
    }
}
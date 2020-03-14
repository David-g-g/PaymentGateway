using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentGateway.Domain.Interfaces.Repository;

namespace PaymentGateway.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly WebApplicationFactory<Startup> webAppFactory;

        protected IntegrationTestBase()
        {
            webAppFactory = new WebApplicationFactory<Startup>();
            
            HttpClient = webAppFactory.CreateClient();            
        }

        public T ResolveService<T>()
        {
            return webAppFactory.Services.GetRequiredService<T>();
        }

        public Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data, IDictionary<string, string> customHeaders = null)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            AddHeaders(customHeaders, content);

            return HttpClient.PostAsync(url, content);
        }

        private static void AddHeaders(IDictionary<string, string> customHeaders, StringContent content)
        {
            if (customHeaders == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> header in customHeaders)
            {
                content.Headers.Add(header.Key, header.Value);
            }
        }

        public static async Task<T> ReadAsJsonAsync<T>(HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
    }
}

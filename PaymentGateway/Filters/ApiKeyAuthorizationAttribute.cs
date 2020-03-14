using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application;
using PaymentGateway.Domain.Interfaces.Repository;

namespace PaymentGateway.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizationAttribute: Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out var merchantApiKey))
            {
                context.Result = new UnauthorizedResult();

                return;
            }

            var merchantRepository = context.HttpContext.RequestServices.GetRequiredService<IMerchantRepository>();

            if (await merchantRepository.GetByApiKey(merchantApiKey) == null) //the repository would cache in the first call the merchant to improve performance in next calls
            {
                context.Result = new UnauthorizedResult();

                return;
            }

            await next();
        }
    }
}

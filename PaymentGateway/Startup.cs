using System;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PaymentGateway.Application;
using PaymentGateway.DataAccess;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Interfaces.AcquiringBank;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Infrastructure;
using PaymentGateway.PipelineBehaviours;
using PaymentGateway.Providers;
using Polly;

namespace PaymentGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment Gateway", Version = "v1" });
                c.OperationFilter<AddApiKeyHeaderParameter>();;
            });

            services.AddMediatR(typeof(Startup));
            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<IPaymentRepository, PaymentRepository>();
            services.AddSingleton<IMerchantRepository, CachedMerchantRepository>();
            services.AddScoped<IAcquiringBank, HsbcAquirerBank>();
            services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddSingleton<ISignatureValidator, HmacValidator>();
            services.AddHttpClient(Constants.HsbcBankhttpClientName, client =>
            {
                client.BaseAddress = new Uri(@"http://www.mocky.io/v2/5e6a44222d000059005fa26b"); //get from config
            })
                .AddTransientHttpErrorPolicy(p=> p.WaitAndRetryAsync(3, _=> TimeSpan.FromSeconds(4)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var swaggerOptions = new SwaggerOptions();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(option => {
                option.RouteTemplate = swaggerOptions.JsonRoute;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description);
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

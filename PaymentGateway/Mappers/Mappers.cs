using System;
using AutoMapper;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Application.Queries.GetPaymentByTransactionId;
using PaymentGateway.Contracts.V1.GetPayment;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;

namespace PaymentGateway.Mappers
{
    public class Mappers:Profile
    {
        public Mappers()
        {
            CreateMap<GetPaymentByTransactionIdQueryResponse, GetPaymentResponse>();
            CreateMap<ProcessPaymentRequest, RequestPaymentCommand>();
        }
    }
}

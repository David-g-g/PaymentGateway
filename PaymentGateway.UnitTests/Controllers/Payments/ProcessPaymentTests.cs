using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentGateway.Application;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Application.Queries;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;
using PaymentGateway.Controllers.V1;
using Xunit;

namespace PaymentGateway.UnitTests.Controllers
{
    public class ProcessPaymentTests
    {
        private ILogger<PaymentsController> _logger;
        private IMediator _mediator;
        private IMapper _mapper;
        private PaymentsController _controller;
        private HttpContext _context;
        private string _apiKey = "GoodApiKey";

        public ProcessPaymentTests()
        {
            _logger = Substitute.For<ILogger<PaymentsController>>();
            _mediator = Substitute.For<IMediator>();
            _mapper = Substitute.For<IMapper>();
            _controller = new PaymentsController(_logger, _mediator, _mapper);
            _context = Substitute.For<HttpContext>();
            _controller.ControllerContext.HttpContext = _context;
            var headers = new HeaderDictionary { KeyValuePair.Create<string, StringValues>(Constants.ApiKeyHeaderName, _apiKey) };
            _context.Request.Headers.Returns(headers);
        }

        [Fact]
        public async Task Apikey_is_populated_in_command()
        {
            var request = new ProcessPaymentRequest();
            var requestPaymentCommand = new RequestPaymentCommand();

            _mapper.Map<RequestPaymentCommand>(request).Returns(requestPaymentCommand);

            await _controller.ProcessPayment(request);

            requestPaymentCommand.ApiKey.Should().Be(_apiKey);
        }        

        [Fact]
        public async Task GivenValidPayment_WhenProcessPaymentIsCalled_ThenSuccessResponseIsReturned()
        {
            var merchantTransactionId = "merchant trans";
            var request = new ProcessPaymentRequest { MerchantTransactionId = merchantTransactionId };
            var requestPaymentCommand = new RequestPaymentCommand();
            var requestPaymentCommandResult = new Result();
            var resultCode = "OK";
            var resultDescription = "desciription";
            var transactionId = Guid.NewGuid();
            var getPaymentByMerchantTransactionIdQueryResponse = new GetPaymentByMerchantTransactionIdQueryResponse()
            {
                IsSuccessfull = true,
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                TransactionId = transactionId
            };

            _mapper.Map<RequestPaymentCommand>(request).Returns(requestPaymentCommand);
            _mediator.Send(requestPaymentCommand).Returns(requestPaymentCommandResult);
            _mediator.Send(Arg.Is<GetPaymentByMerchantTransactionIdQuery>(t => t.MerchantTransactionId.Equals(merchantTransactionId) && t.ApiKey.Equals(_apiKey)))
                 .Returns(new Result<GetPaymentByMerchantTransactionIdQueryResponse>() { Value = getPaymentByMerchantTransactionIdQueryResponse });

            var response = await _controller.ProcessPayment(request);

            var ProcessPaymentResponse = ((CreatedResult)response).Value as ProcessPaymentResponse;

            ProcessPaymentResponse.Should().BeEquivalentTo(new ProcessPaymentResponse
            {
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                TransactionId = transactionId
            });

        }

        [Fact]
        public async Task GivenValidationErrorsInPayment_WhenProcessPaymentIsCalled_BadRequestIsReturnedWithVAlidationErrors()
        {
            var request = new ProcessPaymentRequest();
            var requestPaymentCommand = new RequestPaymentCommand();
            var requestPaymentCommandResult = new Result
            {
                Errors = new List<string> { "Error1"}
            };

            var getPaymentByMerchantTransactionIdQueryResponse = new GetPaymentByMerchantTransactionIdQueryResponse() { IsSuccessfull = true };
            
            _mapper.Map<RequestPaymentCommand>(request).Returns(requestPaymentCommand);
            _mediator.Send(requestPaymentCommand).Returns(requestPaymentCommandResult);
            _mediator.Send(Arg.Any<GetPaymentByMerchantTransactionIdQuery>())
                .Returns(new Result<GetPaymentByMerchantTransactionIdQueryResponse>() { Value = getPaymentByMerchantTransactionIdQueryResponse });

            var response = await _controller.ProcessPayment(request);
            
            var ProcessPaymentResponse = ((BadRequestObjectResult)response).Value as ProcessPaymentResponse;

            ProcessPaymentResponse.Should().BeEquivalentTo(new ProcessPaymentResponse
            {
                ResultCode = "ValidationError",
                ResultDescription = "Error1",
            });

        }

        [Fact]
        public async Task GivenPaymentFail_WhenProcessPaymentIsCalled_ThenBadRequestIsReturnedWithError()
        {
            var request = new ProcessPaymentRequest();
            var requestPaymentCommand = new RequestPaymentCommand();
            var requestPaymentCommandResult = new Result();
            var resultCode = "Error";
            var resultDescription = "Error desciription";
            var transactionId = Guid.NewGuid();
            var getPaymentByMerchantTransactionIdQueryResponse = new GetPaymentByMerchantTransactionIdQueryResponse()
            {
                IsSuccessfull = false,
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                TransactionId = transactionId
            };

            _mapper.Map<RequestPaymentCommand>(request).Returns(requestPaymentCommand);
            _mediator.Send(requestPaymentCommand).Returns(requestPaymentCommandResult);
            _mediator.Send(Arg.Any<GetPaymentByMerchantTransactionIdQuery>())
                .Returns(new Result<GetPaymentByMerchantTransactionIdQueryResponse>() { Value = getPaymentByMerchantTransactionIdQueryResponse });

            var response = await _controller.ProcessPayment(request);

            var ProcessPaymentResponse = ((BadRequestObjectResult)response).Value as ProcessPaymentResponse;

            ProcessPaymentResponse.Should().BeEquivalentTo(new ProcessPaymentResponse
            {
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                TransactionId = transactionId
            });

        }

        [Fact]
        public async Task GivenInternalServerError_WhenProcessPaymentIsCalled_InternalServerErrorIsReturned()
        {
            var request = new ProcessPaymentRequest();
            var requestPaymentCommand = new RequestPaymentCommand();


            _mapper.Map<RequestPaymentCommand>(request).Returns(requestPaymentCommand);
            
            _mediator.Send(requestPaymentCommand).Throws(new Exception("any exception"));
                       
            var response = await _controller.ProcessPayment(request);

            ((StatusCodeResult)response).StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);           
        }
    }
}

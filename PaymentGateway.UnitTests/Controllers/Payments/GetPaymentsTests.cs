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
using PaymentGateway.Application.Queries.GetPaymentByTransactionId;
using PaymentGateway.Contracts.V1.GetPayment;
using PaymentGateway.Controllers.V1;
using Xunit;

namespace PaymentGateway.UnitTests.Controllers
{
    public class GetPaymentsTests
    {
        private ILogger<PaymentsController> _logger;
        private IMediator _mediator;
        private IMapper _mapper;
        private PaymentsController _controller;
        private HttpContext _context;
        private string _apiKey = "GoodApiKey";

        public GetPaymentsTests()
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
        public async Task GivenNotExistingPayment_WhenGetPaymentIsCalled_ThenNotFoundIsReturned()
        {
            var transactionId = Guid.NewGuid();

            _mediator.Send(Arg.Is<GetPaymentByTransactionIdQuery>(r => r.TransactionId.Equals(transactionId) && r.ApiKey.Equals(_apiKey)))
                .Returns(new Result<GetPaymentByTransactionIdQueryResponse>());

            var response = await _controller.GetPayment(transactionId);

            response.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task GivenValidationErrorsInPayment_WhenGetPaymentIsCalled_BadRequestIsReturnedWithVAlidationErrors()
        {
            var validationError = "Validation Error1";

            var transactionId = Guid.NewGuid();

            _mediator.Send(Arg.Is<GetPaymentByTransactionIdQuery>(r => r.TransactionId.Equals(transactionId) && r.ApiKey.Equals(_apiKey)))
                .Returns(new Result<GetPaymentByTransactionIdQueryResponse> { Errors = new List<string> { validationError } });

            var response = await _controller.GetPayment(transactionId);

            var getPaymentResponse = ((BadRequestObjectResult)response).Value as List<string>;

            getPaymentResponse.Should().BeEquivalentTo(new List<string>
            {
               validationError
            });
        }

        [Fact]
        public async Task GivenInternalServerError_WhenGetPaymentIsCalled_InternalServerErrorIsReturned()
        {
            var transactionId = Guid.NewGuid();

            _mediator.Send(Arg.Is<GetPaymentByTransactionIdQuery>(r => r.TransactionId.Equals(transactionId) && r.ApiKey.Equals(_apiKey)))
                .Throws(new Exception("any exception"));

            var response = await _controller.GetPayment(transactionId);

            ((StatusCodeResult)response).StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GivenExistingPayment_WhenGetPaymentIsCalled_ThenPaymentIsReturned()
        {
            var transactionId = Guid.NewGuid();
            var getPaymentByTransactionIdQueryResponse = new GetPaymentByTransactionIdQueryResponse();
            var mappedGetPaymentResponse = new GetPaymentResponse();

            _mediator.Send(Arg.Is<GetPaymentByTransactionIdQuery>(r => r.TransactionId.Equals(transactionId) && r.ApiKey.Equals(_apiKey)))
                .Returns(new Result<GetPaymentByTransactionIdQueryResponse> { Value = getPaymentByTransactionIdQueryResponse });
            _mapper.Map<GetPaymentResponse>(getPaymentByTransactionIdQueryResponse).Returns(mappedGetPaymentResponse);

            var response = await _controller.GetPayment(transactionId);

            response.Should().BeOfType<OkObjectResult>();

            var getPaymentResponse = ((OkObjectResult)response).Value as GetPaymentResponse;

            getPaymentResponse.Should().Be(mappedGetPaymentResponse);
        }
       
    }
}

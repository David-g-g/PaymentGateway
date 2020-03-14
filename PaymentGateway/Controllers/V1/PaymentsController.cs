using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Application.Queries;
using PaymentGateway.Application.Queries.GetPaymentByTransactionId;
using PaymentGateway.Contracts.V1.GetPayment;
using PaymentGateway.Contracts.V1.ProcessPaymentRequest;
using PaymentGateway.Filters;
using PaymentGateway.Helpers;

namespace PaymentGateway.Controllers.V1
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IMapper _mapper;

        public PaymentsController(ILogger<PaymentsController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentRequest request)
        {
            var url = "http://localhost:5001/api/payments/"; // get from confing

            try
            {
                var validationErrors = await _mediator.Send(BuildProcessPaymentRequestCommand(request));

                if (validationErrors.IsAnyError())
                {
                    return BadRequest(FormatValidationErrors(validationErrors));
                }

                var paymentResult = await _mediator.Send(new GetPaymentByMerchantTransactionIdQuery { ApiKey = Request.GetMerchantApiKey(), MerchantTransactionId = request.MerchantTransactionId });

                var response = BuildProcessPaymentResponse(paymentResult.Value);

                if (paymentResult.Value.IsSuccessfull)
                {
                    return Created($"{url}{response.TransactionId}", response);
                }

                return BadRequest(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(StatusCodes.Status500InternalServerError);
            };
        }


        [HttpGet("{transactionId:Guid}")]
        public async Task<IActionResult> GetPayment(Guid transactionId)
        {
            try
            {
                var paymentResponse = await _mediator.Send(new GetPaymentByTransactionIdQuery { ApiKey = Request.GetMerchantApiKey(), TransactionId = transactionId });

                if (paymentResponse.IsAnyError())
                {
                    return BadRequest(paymentResponse.Errors);
                }

                if (paymentResponse.Value != null)
                {
                    return Ok(_mapper.Map<GetPaymentResponse>(paymentResponse.Value));
                }

                return NotFound();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment");
                return StatusCode(StatusCodes.Status500InternalServerError);
            };
        }

        private ProcessPaymentResponse FormatValidationErrors(Result validationErrors)
        {
            return new ProcessPaymentResponse
            {
                ResultCode = "ValidationError",
                ResultDescription = string.Join(" ", validationErrors.Errors),
            };
        }

        private RequestPaymentCommand BuildProcessPaymentRequestCommand(ProcessPaymentRequest request)
        {
            var command = _mapper.Map<RequestPaymentCommand>(request);
            command.ApiKey = Request.GetMerchantApiKey();
            return command;
        }

        private static ProcessPaymentResponse BuildProcessPaymentResponse(GetPaymentByMerchantTransactionIdQueryResponse payment)
        {
            return new ProcessPaymentResponse
            {
                ResultCode = payment.ResultCode,
                ResultDescription = payment.ResultDescription,
                TransactionId = payment.TransactionId
            };
        }
    }
}

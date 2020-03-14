using System;
using FluentAssertions;
using PaymentGateway.Domain.Interfaces.AcquiringBank;
using PaymentGateway.Domain.Models;
using Xunit;

namespace PaymentGateway.UnitTests.Domain
{
    public class PaymentTests
    {
        public PaymentTests()
        {
        }

        [Fact]
        public void RegisterAcuirerResponse_RegistersPaymentInformation()
        {
            var transactionId = "transa";
            var resultCode = "ok";
            var resultDescription = "Result";

            var aquirerResponse = new ProcessPaymentResponse
            {
                TransactionId = transactionId,
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                IsSuccess = true,
            };

            var payment = new Payment();

            payment.RegisterAcquirerResponse(aquirerResponse);

            payment.AcquirerResponse.Should().BeEquivalentTo(new AcquirerResponse
            {
                TransactionId = transactionId,
                ResultCode = resultCode,
                ResultDescription = resultDescription,
                IsSuccess = true,
            }, opt=> opt.Excluding(f=>f.CreatedDate));

        }

        [Fact]
        public void GivenPendingPayment_ThenPendingStatusIsReturned()
        {
            var payment = new Payment();

            payment.GetPaymentStatus().Status.Should().Be(PaymentStatusEnum.Processing);           
        }

        [Fact]
        public void GivenFailedPayment_ThenFailedStatusIsReturned()
        {
            var resultCode = "err";
            var resultDescription = "Result";

            var payment = new Payment() { AcquirerResponse = new AcquirerResponse {IsSuccess = false, ResultCode = resultCode, ResultDescription = resultDescription } };

            payment.GetPaymentStatus().Status.Should().Be(PaymentStatusEnum.FinishedFailed);
            payment.GetPaymentStatus().StatusCode.Should().Be(resultCode);
            payment.GetPaymentStatus().StatusReason.Should().Be(resultDescription);

        }

        [Fact]
        public void GivenSuccessfullPayment_ThenSuccessStatusIsReturned()
        {
            var resultCode = "ok";
            var resultDescription = "good";

            var payment = new Payment() { AcquirerResponse = new AcquirerResponse { IsSuccess = true, ResultCode = resultCode, ResultDescription = resultDescription } };

            payment.GetPaymentStatus().Status.Should().Be(PaymentStatusEnum.FinishedSuccesfully);
            payment.GetPaymentStatus().StatusCode.Should().Be(resultCode);
            payment.GetPaymentStatus().StatusReason.Should().Contain("ayment processed on ");
        }
    }
}
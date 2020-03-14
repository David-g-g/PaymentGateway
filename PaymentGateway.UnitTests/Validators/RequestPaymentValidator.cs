using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Interfaces.Repository;
using PaymentGateway.Domain.Models;
using PaymentGateway.Validators;
using Xunit;

namespace PaymentGateway.UnitTests.Validators
{
    public class RequestPaymentValidatorTests
    {
        private IMerchantRepository _merchantRepository;
        private IPaymentRepository _paymentrepository;
        private ISignatureValidator _signatureValidator;
        private RequestPaymentCommandValidator _validator;

        public RequestPaymentValidatorTests()
        {
            _merchantRepository = Substitute.For<IMerchantRepository>();
            _paymentrepository = Substitute.For<IPaymentRepository>();
            _signatureValidator = Substitute.For<ISignatureValidator>();

            _validator = new RequestPaymentCommandValidator(_signatureValidator, _merchantRepository, _paymentrepository);
        }

        [Fact]
        public void GiveValidCommand_WhenValidateIsCalled_ThenIsValidIsTrue()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void GivenInvalidSignature_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(false);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GivenExistingPayment_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _paymentrepository.GetByMerchantTransactionId(merchant.Id, command.MerchantTransactionId).Returns(new Payment());
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GivenEmptyCardNumber_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.CardNumber = string.Empty;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("123412341234123")]
        [InlineData("1234123412341")]
        [InlineData("12341234123412333")]
        [InlineData("123412341234123A")]
        [InlineData("1224234342342341234123A")]
        [InlineData("1234123A")]
        [InlineData("12341234123")]
        public void GivenInvalidCardNumber_WhenValidateIsCalled_ThenIsValidIsFalse(string cardNumber)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.CardNumber = cardNumber;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GivenEmptyCvvNumber_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.Cvv = string.Empty;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("12A")]
        [InlineData("1")]
        [InlineData("12")]
        [InlineData("1234")]
        public void GivenInvalidCvv_WhenValidateIsCalled_ThenIsValidIsFalse(string cardNumber)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.Cvv = cardNumber;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void GivenInvalidAmount_WhenValidateIsCalled_ThenIsValidIsFalse(decimal amount)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.Amount = amount;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(-1)]
        public void GivenIExpiryMonth_WhenValidateIsCalled_ThenIsValidIsFalse(int month)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.ExpiryMonth = month;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2019)]
        [InlineData(2018)]
        [InlineData(1990)]
        public void GivenIExpirYear_WhenValidateIsCalled_ThenIsValidIsFalse(int year)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.ExpiryYear = year;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GivenEmptyCurrency_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.Currency = string.Empty;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GivenEmptyMerchantTransactionId_WhenValidateIsCalled_ThenIsValidIsFalse()
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.MerchantTransactionId = string.Empty;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AA")]
        [InlineData("AAAA")]
        [InlineData("AAAAA")]
        public void GivenInvalidCurrency_WhenValidateIsCalled_ThenIsValidIsFalse(string currency)
        {
            var apiKey = "new api key";
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, SigningKey = "singkey" };
            var command = BuildValidCommand(apiKey);
            command.Currency = currency;

            _merchantRepository.GetByApiKey(apiKey).Returns(merchant);
            _signatureValidator.IsValidSignature(Arg.Any<Dictionary<string, string>>(), command.Signature, merchant.SigningKey).Returns(true);

            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
        }

        public RequestPaymentCommand BuildValidCommand(string apiKey)
        {
            return new RequestPaymentCommand
            {
                ApiKey = apiKey,
                CardNumber = "1234123412341234",
                Cvv = "123",
                Amount = 10.40m,
                Currency = "EUR",
                ExpiryMonth = 12,
                ExpiryYear = 2023,
                MerchantTransactionId = "Transaction id",
                Signature = "signature"
            };
        }
    }
}

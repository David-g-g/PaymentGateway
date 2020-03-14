using System.Threading.Tasks;
using FluentValidation;
using PaymentGateway.Application.Commands.RequestPayment;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Interfaces.Repository;

namespace PaymentGateway.Validators
{
    public class RequestPaymentCommandValidator:AbstractValidator<RequestPaymentCommand>
    {
        private readonly ISignatureValidator _signatureValidator;
        private readonly IMerchantRepository _merchantRepository;
        public IPaymentRepository _paymentRepository;

        public RequestPaymentCommandValidator(ISignatureValidator signatureValidator, IMerchantRepository merchantRepository, IPaymentRepository paymentRepository)
        {
            _signatureValidator = signatureValidator;
            _merchantRepository = merchantRepository;
            _paymentRepository = paymentRepository;

            ConfigureValidations();
        }

        

        private void ConfigureValidations()
        {
            RuleFor(x => x.MerchantTransactionId)
                            .NotEmpty()
                            .Length(1, 256);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3);

            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .Matches(@"^\d{16}$")
                .Length(16);

            RuleFor(x => x.Cvv)
                .Length(3)
                .Matches(@"^\d{3}$")
                .NotEmpty();

            RuleFor(x => x.ExpiryMonth)
                .GreaterThan(0)
                .LessThan(13);

            RuleFor(x => x.ExpiryYear)
                .GreaterThan(2019);

            RuleFor(x => x.Signature)
                .NotEmpty();

            RuleFor(comand => comand)
                .MustAsync((command, cacelationToken) => IsValidSignature(command))
                .WithMessage("Invalid Signature");

            RuleFor(comand => comand)
                .MustAsync((command, cacelationToken) => PaymentDoesNotExist(command))
                .WithMessage("Payment already processed (or in process)");
        }

        private async Task<bool> IsValidSignature(RequestPaymentCommand command)
        {
            var merchantSigningKey = (await _merchantRepository.GetByApiKey(command.ApiKey)).SigningKey;

            return _signatureValidator.IsValidSignature(command.ToDictionary(), command.Signature, merchantSigningKey);
        }

        private async Task<bool> PaymentDoesNotExist(RequestPaymentCommand command)
        {
            var merchantId = (await _merchantRepository.GetByApiKey(command.ApiKey)).Id;

            return await _paymentRepository.GetByMerchantTransactionId(merchantId, command.MerchantTransactionId) == null;
        }
    }
}

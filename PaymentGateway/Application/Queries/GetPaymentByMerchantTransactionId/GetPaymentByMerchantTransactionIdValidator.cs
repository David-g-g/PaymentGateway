using FluentValidation;

namespace PaymentGateway.Application.Queries.GetPaymentByMerchantTransactionId
{
    public class GetPaymentByMerchantTransactionIdValidator : AbstractValidator<GetPaymentByMerchantTransactionIdQuery>
    {
        public GetPaymentByMerchantTransactionIdValidator()
        {
            RuleFor(r => r.MerchantTransactionId)
                .NotEmpty();
        }
    }
}

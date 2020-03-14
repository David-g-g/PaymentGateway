using FluentValidation;
using PaymentGateway.Application.Queries.GetPaymentByTransactionId;

namespace PaymentGateway.Application.Queries.GetPaymentByMerchantTransactionId
{
    public class GetPaymentByTransactionIdValidator : AbstractValidator<GetPaymentByTransactionIdQuery>
    {
        public GetPaymentByTransactionIdValidator()
        {
            RuleFor(r => r.TransactionId)
                .NotEmpty();
        }
    }
}

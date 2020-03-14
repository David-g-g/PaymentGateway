using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using PaymentGateway.Application;

namespace PaymentGateway.PipelineBehaviours
{
    public class ValidationBehaviour<TRequest, TResponse>:IPipelineBehavior<TRequest, TResponse>
        where TRequest:IRequest<TResponse> where TResponse:Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
            
        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var context = new ValidationContext(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(e => e.Errors)
                .Where(e => e != null)
                .ToList();

            if (failures.Any())
            {
                var result = Activator.CreateInstance<TResponse>();
                result.Errors = failures.Select(f => f.ErrorMessage).ToList();
                return Task.FromResult(result);
            }

            return next();
        }
    }
}

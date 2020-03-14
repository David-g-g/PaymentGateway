using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.Application
{
    public class Result
    {
        public List<string> Errors { get; set; } = new List<string>();

        public bool IsAnyError()
        {
            return Errors.Any();
        }

        internal static Result Empty()
        {
            return new Result();
        }
    }

    public class Result<T>:Result
    {
        public T Value { get; set; }
    }
}

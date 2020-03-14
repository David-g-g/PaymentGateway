using System;
using System.Collections.Generic;
using Xunit;
using PaymentGateway.Infrastructure;
using FluentAssertions;

namespace PaymentGateway.UnitTests
{
    public class HmacValidatorTests
    {
        [Fact]
        public void GivenValidSignature_WhenIsValidIsCalled_ThenReturnIsTrue()
        {
            var validator = new HmacValidator();

            var fields = new Dictionary<string, string>
            {
                {"MerchantTransactionId", "12345678901" },
                {"Amount", "10.00" },
                {"CardNumber", "1234123412341234" },
                {"ExpiryMonth", "10" },
                {"ExpiryYear", "2022" },
                {"Cvv", "123" },
                {"Currency", "EUR" }
            };

            var key = "DFB1EB5485895CFA84146406857104ABB4CBCABDC8AAF103A624C8F6A3EAAB00";
            var originalSignature = "j6Ze59AWImB/ka9AySkvCxbOhvdX0P9yqiojz3vfVlE=";
            validator.IsValidSignature(fields, originalSignature, key).Should().BeTrue();
        }

        [Fact]
        public void GivenInValidSignature_WhenIsValidIsCalled_ThenReturnIsFalse()
        {
            var val = new HmacValidator();

            var fields = new Dictionary<string, string>
            {
                {"Field1", "Value1" },
                {"Field2", "Value2" },
                {"Field3", "Value3" }
            };

            var key = "DFB1EB5485895CFA84146406857104ABB4CBCABDC8AAF103A624C8F6A3EAAB00";
            var originalSignature = "blablablablablbaoWZFUUcY8F9RzjHpNIGd8kEfIkEdKGSbTqeOt2ScocA=";
            val.IsValidSignature(fields, originalSignature, key).Should().BeFalse();
        }

        [Fact]
        public void GivenWrongSigningKey_WhenIsValidIsCalled_ThenReturnIsFalse()
        {
            var val = new HmacValidator();

            var fields = new Dictionary<string, string>
            {
                {"Field1", "Value1" },
                {"Field2", "Value2" },
                {"Field3", "Value3" }
            };

            var key = "DifferentDFB1EB5485895CFA84146406857104ABB4CBCABDC8AAF103A624C8F6A3EAAB00";
            var originalSignature = "oWZFUUcY8F9RzjHpNIGd8kEfIkEdKGSbTqeOt2ScocA=";
            val.IsValidSignature(fields, originalSignature, key).Should().BeFalse();
        }       
    }
}

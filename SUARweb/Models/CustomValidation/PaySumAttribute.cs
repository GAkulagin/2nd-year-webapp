using System;
using System.ComponentModel.DataAnnotations;

namespace SUARweb.Models.CustomValidation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PaySumAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var number = (decimal)value;

            return number > 0;
        }
    }
}

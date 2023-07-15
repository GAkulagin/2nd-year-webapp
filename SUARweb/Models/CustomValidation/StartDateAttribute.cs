using System;
using System.ComponentModel.DataAnnotations;

namespace SUARweb.Models.CustomValidation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class StartDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var date = (DateTime)value;

            return date.Date >= DateTime.Today;
        }
    }
}

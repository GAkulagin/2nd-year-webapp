using System;
using System.ComponentModel.DataAnnotations;

namespace SUARweb.Models.CustomValidation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BuildYearAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var year = (int)value;

            return year >= 0 && year <= DateTime.Today.Year;
        }
    }
}

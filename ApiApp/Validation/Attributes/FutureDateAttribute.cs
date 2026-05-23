using System.ComponentModel.DataAnnotations;

namespace ApiApp.Validation.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateValue)
            {
                return dateValue > DateTime.Now;
            }
            return true; 
        }
    }
}

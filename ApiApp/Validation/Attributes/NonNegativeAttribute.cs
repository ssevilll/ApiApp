using System.ComponentModel.DataAnnotations;

namespace ApiApp.Validation.Attributes
{
    public class NonNegativeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int intValue)
            {
                return intValue >= 0;
            }
            return true; // If not an integer, consider it valid (or you can choose to return false)
        }
    }
}

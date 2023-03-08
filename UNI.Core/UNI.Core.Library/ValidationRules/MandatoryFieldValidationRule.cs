using System;

namespace UNI.Core.Library.ValidationRules
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class MandatoryFieldValidationRule : Attribute, IValidationRule
    {
        public bool Validate(object value, out string message)
        {
            Type type = value.GetType();
            message = string.Empty;
            if (type.Equals(typeof(string)))
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                    return true;
                else
                {
                    message = "This field cannot be void";
                    return false;
                }
            }
            return false;
        }
    }
}

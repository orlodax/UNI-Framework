namespace UNI.Core.Library.ValidationRules
{
    public interface IValidationRule
    {
        bool Validate(object value, out string message);
    }
}

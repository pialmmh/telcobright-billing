namespace FlexValidation
{
    public interface IValidationRule
    {
        bool Validate(object validatableObject);
    }
}
namespace LibraryExtensions
{
    public interface IValidationRule
    {
        string ValidationMessage { get; }
        bool Validate(object validatableObject);
    }
}
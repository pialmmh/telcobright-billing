namespace LibraryExtensions
{
    public class ValidationClause
    {
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
        public ValidationClause(bool isValid, string validationMessage)
        {
            this.IsValid = isValid;
            this.ValidationMessage = validationMessage;
        }
    }
}
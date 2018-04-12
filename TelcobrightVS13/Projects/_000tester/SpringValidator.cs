using Spring.Validation;

namespace Utils
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }


    public class ValidatorTester
    {
        public void TestRequiredValidator()
        {
            User user = new User() { Name = "" };
            BaseValidator baseValidator = new RequiredValidator("Name", null);
            IValidationErrors errors = new ValidationErrors();
            bool valid = baseValidator.Validate(user, errors);
            var x = errors.GetErrors("message");

        }
    }
    public class SpringValidator
    {

    }
}

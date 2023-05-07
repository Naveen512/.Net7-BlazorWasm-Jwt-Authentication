using FluentValidation;

namespace JWT.Auth.BlazorUI.ViewModels.Accounts
{
    public class LoginValidationVm: AbstractValidator<LoginVm>
    {
        public LoginValidationVm()
        {
            RuleFor(_ => _.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid Email");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Invalid Credentials")
                    .MinimumLength(6).WithMessage("Invalid Credentials")
                    .MaximumLength(16).WithMessage("Invalid Credentials")
                    .Matches(@"[A-Z]+").WithMessage("Invalid Credentials")
                    .Matches(@"[a-z]+").WithMessage("Invalid Credentials")
                    .Matches(@"[0-9]+").WithMessage("Invalid Credentials")
                    .Matches(@"[\@\!\?\*\.]+").WithMessage("Invalid Credentials");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result =
            await ValidateAsync(ValidationContext<LoginVm>.CreateWithOptions((LoginVm)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}

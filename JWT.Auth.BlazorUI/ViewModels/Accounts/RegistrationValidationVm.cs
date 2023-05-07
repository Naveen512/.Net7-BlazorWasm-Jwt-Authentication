using FluentValidation;
using System.Text.RegularExpressions;

namespace JWT.Auth.BlazorUI.ViewModels.Accounts
{
    public class RegistrationValidationVm : AbstractValidator<RegistrationVM>
    {
        private readonly HttpClient _httpClient;
        public RegistrationValidationVm(HttpClient httpClient)
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress()
                .MustAsync(async (value, CancellationToken) => await UniqueEmail(value))
                .When(_ => !string.IsNullOrEmpty(_.Email) && Regex.IsMatch(_.Email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase), ApplyConditionTo.CurrentValidator )
                .WithMessage("Email Is Already Exist");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Your password cannot be empty")
                    .MinimumLength(6).WithMessage("Your password length must be at least 6.")
                    .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                    .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                    .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                    .Matches(@"[\@\!\?\*\.]+").WithMessage("Your password must contain at least one (@!? *.).");
            RuleFor(x => x.ConfirmPassword).Equal(_ => _.Password).WithMessage("'Confirm Password' must be equal 'Password'");
            _httpClient = httpClient;

        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result =
            await ValidateAsync(ValidationContext<RegistrationVM>.CreateWithOptions((RegistrationVM)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };

        private async Task<bool> UniqueEmail(string email)
        {
            try
            {
                string url = $"/api/User/unique-user-email?email={email}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return Convert.ToBoolean(content); 
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }
    }
}

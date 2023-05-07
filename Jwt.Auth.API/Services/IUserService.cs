using Jwt.Auth.API.Dtos;

namespace Jwt.Auth.API.Services
{
    public interface IUserService
    {
       Task<(bool IsUserRegistered, string Message)> RegisterNewUserAsync(UserRegistrationDto userRegistration);

        bool CheckUserUniqueEmail(string email);

        Task<(bool IsLoginSuccess, JWTTokenResponseDto TokeResponse)> LoginAsync(LoginDto loginPayload);
    }
}

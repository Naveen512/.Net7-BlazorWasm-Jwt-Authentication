using Jwt.Auth.API.Data;
using Jwt.Auth.API.Data.Entities;
using Jwt.Auth.API.Dtos;
using Jwt.Auth.API.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Jwt.Auth.API.Services
{
    public class UserService : IUserService
    {
        private readonly MyWorldDbContext _worldDbContext;
        private readonly TokenSettings _tokenSettings;
        public UserService(MyWorldDbContext worldDbContext,
            IOptions<TokenSettings> tokenSettings)
        {
            _worldDbContext = worldDbContext;
            _tokenSettings = tokenSettings.Value;
        }

        private User FromUserRegistrationModelToUserModel(UserRegistrationDto userRegistration)
        {
            return new User
            {
                Email = userRegistration.Email,
                FirstName = userRegistration.FirstName,
                LastName = userRegistration.LastName,
                Password = userRegistration.Password,
            };
        }

        private string HashPassword(string plainPassword)
        {
            byte[] salt =new byte[16];
            using(var rng= RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var rfcPassord = new Rfc2898DeriveBytes(plainPassword, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassord.GetBytes(20);

            byte[] passwordHash = new byte[36];
            Array.Copy(salt, 0,passwordHash, 0, 16);
            Array.Copy(rfcPasswordHash, 0, passwordHash, 16, 20);

            return Convert.ToBase64String(passwordHash);
        }
        public async Task<(bool IsUserRegistered, string Message)> RegisterNewUserAsync(UserRegistrationDto userRegistration)
        {
            var isUserExist = _worldDbContext.User.Any(_ => _.Email.ToLower() == userRegistration.Email.ToLower());
            if (isUserExist)
            {
                return (false, "Email Address  Already Registred");
            }

            var newUser = FromUserRegistrationModelToUserModel(userRegistration);
            newUser.Password = HashPassword(newUser.Password);

            _worldDbContext.User.Add(newUser);
            await _worldDbContext.SaveChangesAsync();
            return (true, "Success");
        }

        public bool CheckUserUniqueEmail(string email)
        {
            var userAlreadyExist = _worldDbContext.User.Any(_ => _.Email.ToLower() == email.ToLower());
            return !userAlreadyExist;
        }

        private bool PasswordVerification(string plainPassword, string dbPassword)
        {
            byte[] dbPasswordHash = Convert.FromBase64String(dbPassword);

            byte[] salt = new byte[16];
            Array.Copy(dbPasswordHash, 0,salt, 0, 16);

            var rfcPassord = new Rfc2898DeriveBytes(plainPassword, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassord.GetBytes(20);

            for (int i = 0; i < rfcPasswordHash.Length; i++)
            {
                if (dbPasswordHash[i + 16] != rfcPasswordHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));

            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var cliams = new List<Claim>();
            cliams.Add(new Claim("Sub", user.Id.ToString()));
            cliams.Add(new Claim("FirstName", user.FirstName));
            cliams.Add(new Claim("LastName", user.LastName));
            cliams.Add(new Claim("Email", user.Email));

            var securityToken = new JwtSecurityToken(
                issuer: _tokenSettings.Issuer,
                audience: _tokenSettings.Audience,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials,
                claims: cliams);

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        public async Task<(bool IsLoginSuccess, JWTTokenResponseDto TokeResponse)> LoginAsync(LoginDto loginPayload)
        {
            if(string.IsNullOrEmpty(loginPayload.Email) || string.IsNullOrEmpty(loginPayload.Password))
            {
                return (false, null);
            }

            var user = await _worldDbContext.User.Where(_ => _.Email.ToLower() == loginPayload.Email.ToLower()).FirstOrDefaultAsync();

            if (user == null) { return (false, null); }

            bool validPassword = PasswordVerification(loginPayload.Password, user.Password);
            if(!validPassword) { return (false, null); }

            var jwtAccessToken = GenerateJwtToken(user);

            var result = new JWTTokenResponseDto
            {
                AccessToken = jwtAccessToken,
            };
            return (true, result);
        }
    }
}

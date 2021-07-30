using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Auth.Token;
using System.Text.Json;

namespace Widely.BusinessLogic.Utilities
{
    public class JwtManager
    {
        public readonly IConfiguration _configuration;
        public JwtManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(LogInResponse user)
        {
            Int32.TryParse(_configuration.GetSection("JwtSetting:ExpireMins").Value, out int tokenExpire);
            var appauthorize = JsonSerializer.Serialize(user.AppModule);

            List<Claim> claims = new List<Claim> {
                new Claim (ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim (ClaimTypes.Name, user.Username),
                new Claim (ClaimTypes.Role, user.RoleId?.ToString() ?? ""),
                new Claim ("appauthorize", appauthorize),
                new Claim ("refreshtoken", user.RefreshToken),
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSetting:AccessSecret").Value)
            );

            SigningCredentials creds = new SigningCredentials(
               key, SecurityAlgorithms.HmacSha512Signature
           );

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpire),
                SigningCredentials = creds,
                Issuer = _configuration.GetSection("JwtSetting:Issuer").Value,
                Audience = _configuration.GetSection("JwtSetting:Audience").Value,
                NotBefore = DateTime.UtcNow,
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress, string userAgent, string machineName)
        {
            Int32.TryParse(_configuration.GetSection("JwtSetting:ExpireMins").Value, out int tokenExpire);
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    ExpiresOn = DateTime.Now.AddMinutes(tokenExpire),
                    IssuedOn = DateTime.Now,
                    RequestIp = ipAddress,
                    UserAgent = userAgent,
                    MachineName = machineName
                };
            }
        }
    }
}

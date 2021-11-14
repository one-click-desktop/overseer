using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Backend.Classes;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class LoginService : ILoginService
    {
        private readonly JwtSettings jwtSettings;

        //Fake UserData
        private Dictionary<string, string> fakeUsersPasswords;
        private Dictionary<string, string> fakeUsersRoles;

        public LoginService(IOptions<JwtSettings> jwtSettings)
        {
            this.jwtSettings = jwtSettings.Value;

            fakeUsersPasswords = new Dictionary<string, string>();
            fakeUsersPasswords.Add("user1", "user1_pass");
            fakeUsersPasswords.Add("user2", "user2_pass");
            fakeUsersPasswords.Add("admin1", "admin1_pass");

            fakeUsersRoles = new Dictionary<string, string>();
            fakeUsersRoles.Add("user1", Role.User);
            fakeUsersRoles.Add("user2", Role.User);
            fakeUsersRoles.Add("admin1", Role.Admin);
        }

        public Token Login(Login loginData)
        {
            if (!fakeUsersPasswords.TryGetValue(loginData._Login, out string pass))
                throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized);

            if (pass != loginData.Password)
                throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized);

            // authentication successful so generate jwt token
            string token = GenerateJwtToken(loginData);
            return new Token() { _Token = token };
        }

        private string GenerateJwtToken(Login user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Do ról wystarczy potem tutaj dopisać w Claimach rolę
                //https://jasonwatmore.com/post/2019/10/16/aspnet-core-3-role-based-authorization-tutorial-with-example-api
                //Uwagi do modyfikacji utoryzacji na kazdym jej szczeblu - .NET 5.0
                //https://jasonwatmore.com/post/2021/07/29/net-5-role-based-authorization-tutorial-with-example-api
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user._Login),
                        new Claim(ClaimTypes.Role, fakeUsersRoles[user._Login]),
                    }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Options;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Helpers.Settings;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class TestUserService : IUserService
    {
        private readonly TestDataContext context;
        private readonly IJwtUtils jwtUtils;

        public TestUserService(TestDataContext context,
                               IJwtUtils jwtUtils)
        {
            this.jwtUtils = jwtUtils;
            this.context = context;
        }

        public TokenDTO Login(LoginDTO loginData)
        {
            var user = context.Users.SingleOrDefault(x => x.Username == loginData.Login);

            // validate
            if (user == null || loginData.Password != user.PasswordHash)
                throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized);

            // authentication successful so generate jwt token
            var jwtToken = jwtUtils.GenerateJwtToken(user);

            return new TokenDTO() { Token = jwtToken, Role = user.Role };
        }

        public User GetUserById(Guid guid)
        {
            var user = context.Users.Find(guid);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
    }
}

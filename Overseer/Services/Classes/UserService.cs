using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Options;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class UserService : IUserService
    {
        private DataContext context;
        private IJwtUtils jwtUtils;
        private readonly JwtSettings jwtSettings;

        public UserService(DataContext context,
            IJwtUtils jwtUtils,
            IOptions<JwtSettings> jwtSettings)
        {
            this.jwtSettings = jwtSettings.Value;
            this.jwtUtils = jwtUtils;
            this.context = context;
        }

        public Token Login(Login loginData)
        {
            var user = context.Users.SingleOrDefault(x => x.Username == loginData._Login);

            // validate
            if (user == null || loginData.Password != user.Password)
                throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized);

            // authentication successful so generate jwt token
            var jwtToken = jwtUtils.GenerateJwtToken(user);

            return new Token() { _Token = jwtToken };
        }

        public User GetUserById(int id)
        {
            var user = context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
    }
}

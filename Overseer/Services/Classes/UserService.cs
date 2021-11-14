using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Backend.Classes;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class UserService : IUserService
    {
        private DataContext context;
        private IJwtUtils jwtUtils;
        private readonly JwtSettings jwtSettings;

        //Fake UserData
        private Dictionary<string, User> fakeUsers;

        public UserService(DataContext context,
            IJwtUtils jwtUtils,
            IOptions<JwtSettings> jwtSettings)
        {
            this.jwtSettings = jwtSettings.Value;
            this.jwtUtils = jwtUtils;
            this.context = context;

            fakeUsers = new Dictionary<string, User>();

            fakeUsers.Add("user1", new User() { Id = 1, Username = "user1", Password = "user1_pass", Role = Role.User });
            fakeUsers.Add("user2", new User() { Id = 1, Username = "user2", Password = "user2_pass", Role = Role.User });
            fakeUsers.Add("admin1", new User() { Id = 1, Username = "admin1", Password = "admin1_pass", Role = Role.Admin });
        }

        public Token Login(Login loginData)
        {
            var user = context.Users.SingleOrDefault(x => x.Username == loginData._Login);

            // validate
            if (user == null || loginData.Password != user.Password)
                throw new AppException("Bad credentials");

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

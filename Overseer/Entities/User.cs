using System;
using OneClickDesktop.Api.Models;

namespace OneClickDesktop.Overseer.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public TokenDTO.RoleEnum Role { get; set; }
        public string PasswordHash { get; set; }
    }
}

using System;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class LdapUserService: IUserService
    {
        public TokenDTO Login(LoginDTO loginData)
        {
            //Pobierz dane z ldapa
            throw new NotImplementedException();
        }

        public User GetUserById(Guid guid)
        {
            //Zakladamy, ze w ldapie mozna wyszulkac uzytkownika po guidzie
            throw new NotImplementedException();
        }
    }
}
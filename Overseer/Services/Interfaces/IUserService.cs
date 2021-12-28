
using System;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Entities;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    /// <summary>
    /// Service pilnujący logowania i autoryzacji
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Metoda sprawdza, czy dane logowania sa prawidłowe.
        /// </summary>
        /// <param name="loginData">Dane logowania uzytkownika</param>
        /// <returns>Token przy prawidłowych danych. Null wpw.</returns>
        TokenDTO Login(LoginDTO loginData);

        User GetUserById(Guid guid);
    }
}

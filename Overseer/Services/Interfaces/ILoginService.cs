
using OneClickDesktop.Api.Models;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    /// <summary>
    /// Service pilnujący logowania i autoryzacji
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Metoda sprawdza, czy dane logowania sa prawidłowe.
        /// </summary>
        /// <param name="loginData">Dane logowania uzytkownika</param>
        /// <returns>Token przy prawidłowych danych. Null wpw.</returns>
        Token Login(Login loginData);
    }
}

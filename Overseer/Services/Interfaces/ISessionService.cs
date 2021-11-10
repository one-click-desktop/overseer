
using OneClickDesktop.Api.Models;
using OneClickDesktop.Backend.Classes;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    /// <summary>
    /// Service zarządzający sesjami uzytkowników w systemie.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Metoda przekazuje do reszty systemu prośbe o utowrzenie sesji dla danego uzytkownika.
        /// </summary>
        /// <param name="type">Typ sesji do utworzenia</param>
        /// <param name="userGuid">Identyfikator uzytkownika, dla którego utworzyć sesję.</param>
        /// <returns>Identyfikator sesji do późniejszego odwołania się.</returns>
        string RequestSession(MachineType type, string userGuid);

        /// <summary>
        /// Metoda sprawdza czy sesja o danym identyfikatorze jest juz w pełni utworzona w systemie.
        /// Wtedy zwraca obiekt reprezentujący sesję.
        /// </summary>
        /// <param name="sessionGuid">Identyfikator sesji.</param>
        /// <param name="userGuid">Identyfikator uzytkownika, który pyta</param>
        /// <returns>Zwraca obiekt reprezentujący sesję, jeżeli jest w pełni utworzona. Wpw. zwraca <c>null</c>.</returns>
        Session AskForSession(string sessionGuid, string userGuid);

        /// <summary>
        /// Metoda zgłasza do systemu prośbę o anulowanie sesji o danym identyfikatorze.
        /// </summary>
        /// <param name="sessionGuid">identyfikator sesji, która jets w trakcie tworzenia.</param>
        /// <param name="userGuid">Identyfikator proszącego uzytkownika.</param>
        void CancelSession(string sessionGuid, string userGuid);
    }
}

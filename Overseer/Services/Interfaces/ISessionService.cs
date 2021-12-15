
using System;
using OneClickDesktop.Api.Models;

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
        /// <returns>Utworzona sesja (lub istniejąca nadająca się do użycia)</returns>
        SessionDTO RequestSession(MachineTypeDTO  type, Guid userGuid);

        /// <summary>
        /// Metoda sprawdza czy sesja o danym identyfikatorze jest juz w pełni utworzona w systemie.
        /// Wtedy zwraca obiekt reprezentujący sesję.
        /// </summary>
        /// <param name="sessionGuid">Identyfikator sesji.</param>
        /// <param name="userGuid">Identyfikator uzytkownika, który pyta</param>
        /// <returns>Zwraca obiekt reprezentujący sesję, częściową lub pełną.</returns>
        SessionDTO  AskAboutSession(Guid sessionGuid, Guid userGuid);

        /// <summary>
        /// Metoda zgłasza do systemu prośbę o anulowanie sesji o danym identyfikatorze.
        /// </summary>
        /// <param name="sessionGuid">identyfikator sesji, która jets w trakcie tworzenia.</param>
        /// <param name="userGuid">Identyfikator proszącego uzytkownika.</param>
        void CancelSession(Guid sessionGuid, Guid userGuid);
    }
}

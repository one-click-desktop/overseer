using System.Collections.Generic;
using OneClickDesktop.Api.Models;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    /// <summary>
    /// Service, który dostaje sie do modelu systemu i dostarcza dane dla klientów.
    /// </summary>
    public interface IResourcesService
    {
        /// <summary>
        /// Metoda ma za zadanie zwrócić wszystkie zasoby w systemie.
        /// Docelowow dane mają wylądowac w panelu administracyjnym.
        /// </summary>
        /// <returns>Obiekt zawierający wszystkei zasoby w systemie.</returns>
        TotalResourcesDTO GetAllResources();

        /// <summary>
        /// Metoda ma zwracać informacje o dostępności wsyztskich typów maszyn.
        /// Docelowo dane maja wyladowac w aplikacji klienckiej.
        /// </summary>
        /// <returns>Kolekcja obiektów, które opisują licznośc i typ maszyny.</returns>
        IEnumerable<MachineDTO> GetMachinesInfo();
    }
}

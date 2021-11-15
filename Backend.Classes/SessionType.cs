using System;
using System.Security.Cryptography;
using System.Text;

namespace OneClickDesktop.Backend.Classes
{
    /// <summary>
    /// Struktura z nazwy sesji generuje skrót i on jest obowiązującym identyfikatorem typu sesji.
    /// </summary>
    /// <remarks> Aktualnie nie używana! (KS: 11/10/2021) </remarks>
    public struct SessionType
    {
        public string SessionTypeName { get; private set; }
        public long SessionHash { get; private set; }

        public SessionType(string typeName)
        {
            SessionTypeName = typeName;
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(SessionTypeName));
            SessionHash = BitConverter.ToInt64(hash, 0);
        }
    }
}

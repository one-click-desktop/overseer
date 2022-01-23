namespace OneClickDesktop.Overseer.Helpers.Settings
{
    public class LdapSettings
    {
        public string LdapHostname { get; set; } = "localhost";

        public int LdapPort { get; set; } = 389;

        public string UserSearchBase { get; set; } = "ou=users,dc=example,dc=org";

        public string Domain { get; set; } = "dc=example,dc=org";

        public bool UseSsl { get; set; } = false;

        public string UserGuidAttribute { get; set; } = "guid";

        public string UserGroupAttribute { get; set; } = "gidNumber";

        public string UserNameAttribute { get; set; } = "uid";

        public int UserGroupNumber { get; set; } = 502;

        public int AdminGroupNumber { get; set; } = 501;

        public string ReadOnlyUserName { get; set; } = "readonly";

        public string ReadOnlyUserPassword { get; set; } = "readonly";
    }
}
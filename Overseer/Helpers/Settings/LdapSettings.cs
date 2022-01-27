namespace OneClickDesktop.Overseer.Helpers.Settings
{
    /// <summary>
    /// Configuration for LDAP integration
    /// </summary>
    public class LdapSettings
    {
        /// <summary>
        /// LDAP access address
        /// </summary>
        public string LdapHostname { get; set; } = "localhost";

        /// <summary>
        /// LDAP access port
        /// </summary>
        public int LdapPort { get; set; } = 389;

        /// <summary>
        /// Base location to search for users (as DN)
        /// </summary>
        public string UserSearchBase { get; set; } = "ou=users,dc=example,dc=org";

        /// <summary>
        /// LDAP domain name (as DN)
        /// </summary>
        public string Domain { get; set; } = "dc=example,dc=org";

        /// <summary>
        /// Specifies if SSL should be used to connect to LDAP
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Name of attribute holding user GUID
        /// </summary>
        public string UserGuidAttribute { get; set; } = "guid";

        /// <summary>
        /// Name of attribute holding user group ID
        /// </summary>
        public string UserGroupAttribute { get; set; } = "gidNumber";

        /// <summary>
        /// Name of attribute holding userName
        /// </summary>
        public string UserNameAttribute { get; set; } = "uid";

        /// <summary>
        /// ID of OneClickDesktop users group
        /// </summary>
        public int UserGroupNumber { get; set; } = 502;

        /// <summary>
        /// ID of OneClickDesktop admins group
        /// </summary>
        public int AdminGroupNumber { get; set; } = 501;

        /// <summary>
        /// Name of readOnly user used for lookup (as DN)
        /// </summary>
        public string ReadOnlyUserName { get; set; } = "readonly";

        /// <summary>
        /// Password of readOnly user used for lookup
        /// </summary>
        public string ReadOnlyUserPassword { get; set; } = "readonly";
    }
}
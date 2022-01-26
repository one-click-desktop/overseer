using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Helpers.Settings;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class LdapUserService : IUserService, IDisposable
    {
        private readonly IJwtUtils jwtUtils;
        private readonly IOptions<LdapSettings> conf;
        private readonly ILogger<SystemModelService> logger;

        private readonly LdapDirectoryIdentifier ldapServer;
        private readonly NetworkCredential readOnlyCredential;
        private readonly string[] ldapAttributes;

        private static LdapConnection readOnlyConnection;

        public LdapUserService(IJwtUtils jwtUtils,
                               ILogger<SystemModelService> logger,
                               IOptions<LdapSettings> ldapConfig)
        {
            this.jwtUtils = jwtUtils;
            this.logger = logger;
            conf = ldapConfig;

            ldapServer = new LdapDirectoryIdentifier(conf.Value.LdapHostname, conf.Value.LdapPort);
            readOnlyCredential = new NetworkCredential($"cn={conf.Value.ReadOnlyUserName},{conf.Value.Domain}",
                                                       conf.Value.ReadOnlyUserPassword);

            ldapAttributes = new string[]
            {
                conf.Value.UserGuidAttribute,
                conf.Value.UserNameAttribute,
                conf.Value.UserGroupAttribute
            };
        }

        /// <summary>
        /// Login user into system. Verification is made by connected AD
        /// </summary>
        public TokenDTO Login(LoginDTO loginData)
        {
            var name = $"cn={loginData.Login},{conf.Value.UserSearchBase}";
            try
            {
                using var connection = CreateConnection(name, loginData.Password);
                var user = GetUserFromLdap(name, null, connection);

                var jwtToken =
                    jwtUtils.GenerateJwtToken(
                        user ?? throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized));

                return new TokenDTO() { Token = jwtToken, Role = user.Role };
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Exception on connection to LDAP");
                throw new ErrorHttpException("Bad credentials", HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Get specified user from AD
        /// </summary>
        public User GetUserById(Guid guid)
        {
            try
            {
                var connection = GetReadOnlyConnection();
                var filter = $"({conf.Value.UserGuidAttribute}={guid})";
                var user = GetUserFromLdap(conf.Value.UserSearchBase, filter, connection);
                return user ?? throw new KeyNotFoundException("User not found");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Exception on connection to LDAP");
                throw new KeyNotFoundException("User not found");
            }
        }

        /// <summary>
        /// Get or create connection used for readonly access to AD
        /// </summary>
        private LdapConnection GetReadOnlyConnection()
        {
            if (readOnlyConnection != null)
            {
                return readOnlyConnection;
            }

            readOnlyConnection = new LdapConnection(ldapServer, readOnlyCredential, AuthType.Basic);
            readOnlyConnection.SessionOptions.AutoReconnect = true;
            readOnlyConnection.SessionOptions.TcpKeepAlive = true;
            readOnlyConnection.SessionOptions.SecureSocketLayer = conf.Value.UseSsl;
            readOnlyConnection.SessionOptions.ProtocolVersion = 3;
            readOnlyConnection.AutoBind = true;
            
            readOnlyConnection.Bind();
            return readOnlyConnection;
        }

        /// <summary>
        /// Create new connection with specified credentials
        /// </summary>
        private LdapConnection CreateConnection(string userName, string password)
        {
            var credential = new NetworkCredential(userName, password);
            var connection = new LdapConnection(ldapServer, credential, AuthType.Basic);
            connection.SessionOptions.SecureSocketLayer = conf.Value.UseSsl;
            connection.SessionOptions.ProtocolVersion = 3;

            connection.Bind();
            return connection;
        }

        /// <summary>
        /// Get user from AD, or null if fails
        /// </summary>
        private User? GetUserFromLdap(string distinguishedName, string ldapFilter, DirectoryConnection connection)
        {
            var request = new SearchRequest(distinguishedName, ldapFilter, SearchScope.Subtree, ldapAttributes);
            var response = connection.SendRequest(request);

            if (response is SearchResponse { ResultCode: ResultCode.Success } searchResponse &&
                searchResponse.Entries.Count != 0)
            {
                return CreateUserFromAttributes(distinguishedName, searchResponse.Entries[0].Attributes);
            }

            logger.LogWarning($"No response for dn: {distinguishedName}, filter: {ldapFilter}");
            return null;
        }

        /// <summary>
        /// Create user object from AD user attributes
        /// </summary>
        private User? CreateUserFromAttributes(string userName, SearchResultAttributeCollection attributes)
        {
            if (!Guid.TryParse(GetAttributeValue(attributes[conf.Value.UserGuidAttribute]), out var guid))
            {
                logger.LogError($"Failed to parse guid for {userName}");
                return null;
            }

            if (!int.TryParse(GetAttributeValue(attributes[conf.Value.UserGroupAttribute]), out var groupNumber))
            {
                logger.LogError($"Failed to parse group number for {userName}");
                return null;
            }

            TokenDTO.RoleEnum? role;
            if ((role = ResolveRoleForGroup(groupNumber)) == null)
            {
                logger.LogError($"Unrecognised role {groupNumber} for {userName}");
                return null;
            }

            var name = GetAttributeValue(attributes[conf.Value.UserNameAttribute]);

            return new User()
            {
                Id = guid, Role = role.Value, Username = name
            };
        }

        /// <summary>
        /// Parse attribute into string
        /// </summary>
        private string GetAttributeValue(DirectoryAttribute attribute)
        {
            return attribute?.Count != 0 ? attribute[0].ToString() : null;
        }

        /// <summary>
        /// Resolve user role based on AD role
        /// </summary>
        private TokenDTO.RoleEnum? ResolveRoleForGroup(int groupNumber)
        {
            if (groupNumber == conf.Value.AdminGroupNumber)
            {
                return TokenDTO.RoleEnum.Admin;
            }

            if (groupNumber == conf.Value.UserGroupNumber)
            {
                return TokenDTO.RoleEnum.User;
            }

            return null;
        }

        public void Dispose()
        {
            readOnlyConnection?.Dispose();
            readOnlyConnection = null;
        }
    }
}
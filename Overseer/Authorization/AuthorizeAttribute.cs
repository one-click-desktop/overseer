using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers.Exceptions;

namespace OneClickDesktop.Overseer.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<TokenDTO.RoleEnum> _roles;

        public AuthorizeAttribute(params TokenDTO.RoleEnum[] roles)
        {
            _roles = roles ?? new TokenDTO.RoleEnum[] { };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // authorization
            var user = (User)context.HttpContext.Items["User"];

            if (user == null)
                throw new UnathorizedHttpException();

            if (_roles.Any() && !_roles.Contains(user.Role))
                throw new UnathorizedHttpException();
        }
    }
}

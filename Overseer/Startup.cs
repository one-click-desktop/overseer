using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Classes;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.Api.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OneClickDesktop.Overseer.Helpers.Settings;

namespace OneClickDesktop.Overseer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDataContext>();
            services.AddCors();
            services.AddControllers().AddJsonOptions(x =>
            {
                // serialize enums as strings in api responses (e.g. Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            
            // configure strongly typed settings object
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
            services.Configure<OneClickDesktopSettings>(Configuration.GetSection("OneClickDesktop"));
            services.Configure<LdapSettings>(Configuration.GetSection("LDAP"));
            
            //singleton - model (zapytania publiczne muszą byc thread-safe!!!)
            services.AddSingleton<ISystemModelService, SystemModelService>();
            //singleton - rabbit receiver/sender(oddzielny watek)
            services.AddSingleton<IVirtualizationServerConnectionService, VirtualizationServerConnectionService>();
            services.AddSingleton<ISessionProcessService, SessionProcessService>();
            services.AddSingleton<IMachineService, MachineService>();
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IResourcesService, ResourcesService>();
            services.AddScoped<ISessionService, SessionService>();

            var mockUsers = (bool) (Configuration.GetValue(typeof(bool), "mockUsers") ?? false);
            if (mockUsers)
            {
                services.AddScoped<IUserService, TestUserService>();
            }
            else
            {
                services.AddScoped<IUserService, LdapUserService>();
            }
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Overseer", Version = "v3.0.3" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TestDataContext context)
        {
            CreateTestUsers(context);

            //Wymuszenie uruchomienia modułu do komunikajci z rabbitem
            app.ApplicationServices.GetService<IVirtualizationServerConnectionService>();
            app.ApplicationServices.GetService<IMachineService>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Overseer v1"));
            }
            
            app.UseCors(x => x
                             .AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader());
            app.UseRouting();

            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();
            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void CreateTestUsers(TestDataContext context)
        {
            // add hardcoded test users to db on startup
            var testUsers = new List<User>
            {
                new User() { Id = Guid.Parse("f91649fb-de33-49ab-82ba-75d2171f494e"), Username = "vagrant", PasswordHash = "vagrant", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.Parse("73b55082-a92d-40b2-8376-527c63c2948a"), Username = "user1", PasswordHash = "user1_pass", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.Parse("437750eb-e40d-4d17-9510-1b9241ec37fe"), Username = "user2", PasswordHash = "user2_pass", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.Parse("e56497b3-0477-4cf4-8075-e539de14b730"), Username = "admin1", PasswordHash = "admin1_pass", Role = TokenDTO.RoleEnum.Admin }
            };
            context.Users.AddRange(testUsers);
            context.SaveChanges();
        }
    }
}

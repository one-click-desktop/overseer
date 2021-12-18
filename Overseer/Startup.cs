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
using System.Diagnostics;
using System.Text.Json.Serialization;

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
            services.AddDbContext<DataContext>();
            services.AddCors();
            services.AddControllers().AddJsonOptions(x =>
            {
                // serialize enums as strings in api responses (e.g. Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // configure strongly typed settings object
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
            
            //singleton - model (zapytania publiczne muszą byc thread-safe!!!)
            services.AddSingleton<ISystemModelService, SystemModelService>();
            //singleton - rabbit receiver/sender(oddzielny watek)
            services.AddSingleton<IVirtualizationServerConnectionService, VirtualizationServerConnectionService>();
            services.AddSingleton<ISessionProcessService, SessionProcessService>();
            services.AddSingleton<IMachineService, MachineService>();
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IResourcesService, ResourcesService>();
            services.AddScoped<ISessionService, SessionService>();
            
            //var test = new VirtualizationServerConnectionService(new SystemModelService());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Overseer", Version = "v3.0.3" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext context)
        {
            createTestUsers(context);

            //Wymuszenie uruchomienia modułu do komunikajci z rabbitem
            app.ApplicationServices.GetService<IVirtualizationServerConnectionService>();
            app.ApplicationServices.GetService<IMachineService>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Overseer v1"));
            }

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
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

        private void createTestUsers(DataContext context)
        {
            // add hardcoded test users to db on startup
            var testUsers = new List<User>
            {
                new User() { Id = Guid.NewGuid(), Username = "vagrant", Password = "vagrant", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.NewGuid(), Username = "user1", Password = "user1_pass", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.NewGuid(), Username = "user2", Password = "user2_pass", Role = TokenDTO.RoleEnum.User },
                new User() { Id = Guid.NewGuid(), Username = "admin1", Password = "admin1_pass", Role = TokenDTO.RoleEnum.Admin }
            };
            context.Users.AddRange(testUsers);
            context.SaveChanges();
        }
    }
}

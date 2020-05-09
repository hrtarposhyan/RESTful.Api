using Library.Api.Entities;
using Library.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Library.Api.Helpers;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Library.Api
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
           services.AddMvc(setupAction=>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                //setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());


            })
            .AddNewtonsoftJson(options=>
            {
                options.SerializerSettings.ContractResolver=
                new CamelCasePropertyNamesContractResolver();
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            services.AddDbContext<LibraryContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("libraryDBConnectionString")));

            // register the repository

            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            //{
            //    var actionContext =
            //    implementationFactory.GetService<IActionContextAccessor>().ActionContext;
            //    return new UrlHelper(actionContext);
            //});

            services.AddScoped<IUrlHelper>(x =>
            {
                var actionContext =
                x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory,
            LibraryContext libraryContext)
        {
           //loggerFactory.AddConsole();
           //loggerFactory.AddDebug(LogLevel.Information);

           //loggerFactory.AddProvider(new NLog.Extensions.NLogLoggerProvider());
           //loggerFactory.AddNLog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exeptionHendlerFeature=context.Features.Get<IExceptionHandlerFeature>();
                        if (exeptionHendlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exeption logger");
                            logger.LogError(500,
                                exeptionHendlerFeature.Error,
                                exeptionHendlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happend.Try again later..");
                    });
                });
            }
            // init Database
           libraryContext.EnsureSeedDataForContext();


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}

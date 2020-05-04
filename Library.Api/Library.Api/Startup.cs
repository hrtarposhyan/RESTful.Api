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
           services.AddControllers(setupAction=>
            {
                setupAction.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters()
            .AddNewtonsoftJson();

            services.AddDbContext<LibraryContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("libraryDBConnectionString")));
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory,
            LibraryContext libraryContext)
        {
            //loggerFactory.AddConsole();
            //loggerFactory.AddDebug(LogLevel.Information);
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

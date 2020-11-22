using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using LapTimerServer.Lib;
using Microsoft.Extensions.Logging;
using LapTimerServer.Lib.Audio.CrossPlatform;
using LapTimerServer.Lib.Audio;

namespace LapTimerServer
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
            services.AddControllers();
            services.AddSingleton<RaceManager>();
            services.AddSingleton((serviceProvider) =>
            {
                AudioFilePlayerFactory audioFilePlayerFactory = new AudioFilePlayerFactory(serviceProvider.GetRequiredService<ILogger<AudioFilePlayerFactory>>());
                IAudioFilePlayer audioPlayer = audioFilePlayerFactory.CreateWavPlayer();
                return new LapTimeAnnouncer(audioPlayer, @"Lib\Audio\WavFiles");
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "RaceTimer API",
                    Description = "A ASP.NET Core Web API for managing a IoT race timers",
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //     app.UseHttpsRedirection(); // only using http over local network for now

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceTimer API v1");
                config.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
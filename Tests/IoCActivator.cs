using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nexRemoteFree.Agent.Interfaces;
using nexRemoteFree.Agent.Services;
using nexRemoteFree.Server.API;
using nexRemoteFree.Server.Data;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Models;
using nexRemoteFree.Shared.Utilities;
using System;

namespace nexRemoteFree.Tests
{
    [TestClass]
    public class IoCActivator
    {
        public static IServiceProvider ServiceProvider { get; set; }
        private static IWebHostBuilder builder;

        public static void Activate()
        {
            if (builder is null)
            {
                builder = WebHost.CreateDefaultBuilder()
                   .UseStartup<Startup>()
                   .CaptureStartupErrors(true);

                builder.Build();
            }
        }


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Activate();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDb>(options =>
            {
                options.UseInMemoryDatabase("nexRemoteFree");
            });

            services.AddScoped(p =>
                p.GetRequiredService<IDbContextFactory<AppDb>>().CreateDbContext());

            services.AddIdentity<nexRemoteFreeUser, IdentityRole>(options => options.Stores.MaxLengthForKeys = 128)
             .AddEntityFrameworkStores<AppDb>()
             .AddDefaultUI()
             .AddDefaultTokenProviders();

            services.AddTransient<IDataService, DataService>();
            services.AddTransient<IApplicationConfig, ApplicationConfig>();
            services.AddTransient<IEmailSenderEx, EmailSenderEx>();

            if (EnvironmentHelper.IsWindows)
            {
                services.AddTransient<IDeviceInformationService, DeviceInformationServiceWin>();
            }
            else if (EnvironmentHelper.IsLinux)
            {
                services.AddTransient<IDeviceInformationService, DeviceInformationServiceLinux>();
            }

            IoCActivator.ServiceProvider = services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }


}

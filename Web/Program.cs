﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).
                Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true);
            })
            .UseStartup<Startup>();
    }
}
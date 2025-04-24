using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NeoServiceLayer.Api
{
    public class SimpleProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Neo Service Layer API (Simple Version)...");
            
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            var app = builder.Build();
            
            // Configure the HTTP request pipeline
            app.UseSwagger();
            app.UseSwaggerUI();
            
            app.UseHttpsRedirection();
            app.UseAuthorization();
            
            // Add a simple health check endpoint
            app.MapGet("/health", () => "Healthy");
            
            // Add a simple API endpoint
            app.MapGet("/api/status", () => new { Status = "OK", Message = "Neo Service Layer API is running", Timestamp = DateTime.UtcNow });
            
            Console.WriteLine("Neo Service Layer API started successfully.");
            
            app.Run();
        }
    }
}

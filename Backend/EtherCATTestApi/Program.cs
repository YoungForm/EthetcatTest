﻿using Microsoft.EntityFrameworkCore;
using EtherCATTestApi.DataAccess.DbContext;
using EtherCATTestApi.EtherCATTestEngine;
using EtherCATTestApi.EtherCATTestEngine.StateMachine;
using EtherCATTestApi.EtherCATTestEngine.Mailbox;
using EtherCATTestApi.EtherCATTestEngine.DeviceIdentification;
using EtherCATTestApi.EtherCATTestEngine.ESI;
using EtherCATTestApi.EtherCATTestEngine.ObjectDictionary;
using EtherCATTestApi.EtherCATTestEngine.DiffTool;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ethercat_test-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting EtherCAT Test API");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog
    builder.Host.UseSerilog();
    
    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp",
            policy =>
            {
                policy.WithOrigins("http://localhost:5173") // React app default port
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
    });
    
    // Configure database context with SQLite
    builder.Services.AddDbContext<EtherCATDbContext>(options =>
        options.UseSqlite("Data Source=ethercat_test.db"));
    
    // Register EtherCAT test engine services
builder.Services.AddScoped<EtherCATCommManager>();
builder.Services.AddScoped<StateValidator>();
builder.Services.AddScoped<CoETester>();
builder.Services.AddScoped<VendorIdValidator>();
builder.Services.AddScoped<ProductCodeValidator>();
builder.Services.AddScoped<ESIParser>();
builder.Services.AddScoped<ESIValidator>();
builder.Services.AddScoped<ObjectDictionaryValidator>();
builder.Services.AddScoped<ConfigComparator>();
    
    // Add controllers
    builder.Services.AddControllers();
    
    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.UseHttpsRedirection();
    
    // Use CORS
    app.UseCors("AllowReactApp");
    
    // Map controllers
    app.MapControllers();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

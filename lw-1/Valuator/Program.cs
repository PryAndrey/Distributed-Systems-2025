using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
    });

var redisMainConnStr = builder.Configuration["DB_MAIN"] ?? Environment.GetEnvironmentVariable("DB_MAIN");
var redis = ConnectionMultiplexer.Connect(redisMainConnStr);
builder.Services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("Valuator");

var rabbitMqUser = builder.Configuration["RABBITMQ_USER"] ?? Environment.GetEnvironmentVariable("RABBITMQ_USER");
var rabbitMqPass = builder.Configuration["RABBITMQ_PASS"] ?? Environment.GetEnvironmentVariable("RABBITMQ_PASS");

var factory = new ConnectionFactory
{
    HostName = "rabbitmq",
    UserName = rabbitMqUser,
    Password = rabbitMqPass
};

var rabbitMqConnection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(rabbitMqConnection);

builder.Services.AddSingleton<IDBService, Redis>();
builder.Services.AddSingleton<IMessageQueueService, MessageQueue>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
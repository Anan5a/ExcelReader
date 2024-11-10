using DataAccess;
using ExcelReader.BackgroundWorkers;
using ExcelReader.Queues;
using ExcelReader.Realtime;
using IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Text;
using System.Text.Json.Serialization;
using Utility;

var builder = WebApplication.CreateBuilder(args);

//hosting config
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 2_000;
});
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();





builder.Services.AddSingleton<IMyDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new MyDbConnection(connectionString);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddSingleton<ICallQueue<QueueModel>, CallQueue<QueueModel>>();
builder.Services.AddSingleton<AgentQueue>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDevOrigin",
        builder => builder.WithOrigins([
            "http://127.0.0.1:4200",
            "https://127.0.0.1:4200",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://192.168.100.52:4200",
            "https://192.168.100.52:4200",
            "http://192.168.0.101:4200",
            "https://192.168.0.101:4200",
            "https://filekeeper-snowy.vercel.app"
            ])
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtAuthConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtAuthConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtAuthConfig:SigningKey"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Realtime"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };


});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole(UserRoles.Admin));

    options.AddPolicy("UserPolicy", policy =>
        policy.RequireRole(UserRoles.User));

    options.AddPolicy("SuperAdminPolicy", policy =>
        policy.RequireRole(UserRoles.SuperAdmin));
});

builder.Services.AddHostedService<BackgroundCallQueueProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //prod *must* use real https certs, this bypass https in prod,docker container
    app.UseHttpsRedirection();
}
app.UseCors("AllowLocalDevOrigin");

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<SimpleHub>("/Realtime");

app.Run();
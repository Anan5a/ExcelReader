using DataAccess;
using ExcelReader.Realtime;
using IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Utility;

var builder = WebApplication.CreateBuilder(args);

//hosting config
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 2_000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 2_000;

});
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDevOrigin",
        builder => builder.WithOrigins(["http://127.0.0.1:4200", "http://localhost:4200"])
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowLocalDevOrigin");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.MapHub<SimpleHub>("/Realtime");

app.Run();

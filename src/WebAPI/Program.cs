using System.Text;
using Application;
using Application.Mapping;
using Domain.Common;
using Domain.Configurations;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMemoryCache()
    .AddAutoMapper(_ => {}, typeof(MappingProfile).Assembly)
    .AddTransient<ExceptionHandlingMiddleware>();

builder.Services
    .Configure<JwtSettings>(builder.Configuration.GetSection(SettingSectionNames.JwtSettings))
    .Configure<CorsSettings>(builder.Configuration.GetSection(SettingSectionNames.CorsSettings))
    .Configure<MongoDbSettings>(builder.Configuration.GetSection(SettingSectionNames.MongoDbSettings))
    .Configure<CookieSettings>(builder.Configuration.GetSection(SettingSectionNames.CookieSettings))
    .Configure<GithubSettings>(builder.Configuration.GetSection(SettingSectionNames.GithubSettings))
    .Configure<SupabaseSettings>(builder.Configuration.GetSection(SettingSectionNames.SupabaseSettings))
    .AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddJwtBearer(opts =>
    {
        var jwtSettings = builder.Configuration.GetSection(SettingSectionNames.JwtSettings).Get<JwtSettings>();
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.AccessTokenSecretKey)),
        };
    });

builder.Services
    .AddOpenApi()
    .AddCors(opts =>
    {
        opts.AddDefaultPolicy(policy =>
        {
            var corsSettings = builder.Configuration.GetSection(SettingSectionNames.CorsSettings).Get<CorsSettings>();
            policy
                .WithMethods([.. corsSettings.Methods])
                .AllowAnyOrigin()
                .AllowAnyHeader();

        });
    })
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddControllers()
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
        opts.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();

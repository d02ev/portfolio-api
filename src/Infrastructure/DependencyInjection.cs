using Application.Helpers;
using Application.Repositories;
using Application.Integrations;
using Domain.Common;
using Domain.Configurations;
using Infrastructure.Helpers;
using Infrastructure.Repositories;
using Infrastructure.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var mongoDbSettings = configuration.GetSection(SettingSectionNames.MongoDbSettings).Get<MongoDbSettings>();

    services
      .AddSingleton<IMongoClient>(_ => new MongoClient(mongoDbSettings!.Url))
      .AddScoped(provider =>
      {
        var client = provider.GetRequiredService<IMongoClient>();
        return client.GetDatabase(mongoDbSettings!.Db);
      })
      .AddScoped<IAuthHelper, AuthHelper>()
      .AddScoped<IUserRepository, UserRepository>()
      .AddScoped<ITechStackRepository, TechStackRepository>()
      .AddScoped<IProjectRepository, ProjectRepository>()
      .AddScoped<IExperienceRepository, ExperienceRepository>()
      .AddScoped<IContactRepository, ContactRepository>()
      .AddScoped<IEducationRepository, EducationRepository>()
      .AddScoped<IAboutRepository, AboutRepository>()
      .AddScoped<IResumeRepository, ResumeRepository>()
      .AddScoped<ISupabaseIntegration, SupabaseIntegration>()
      .AddScoped<IAiIntegration, AiIntegration>()
      .AddScoped<IGithubIntegration, GithubIntegration>();

    return services;
  }
}
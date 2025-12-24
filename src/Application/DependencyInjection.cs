using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services
      .AddScoped<IUserService, UserService>()
      .AddScoped<ITechStackService, TechStackService>()
      .AddScoped<IAboutService, AboutService>()
      .AddScoped<IProjectService, ProjectService>()
      .AddScoped<IExperienceService, ExperienceService>()
      .AddScoped<IContactService, ContactService>()
      .AddScoped<IEducationService, EducationService>()
      .AddScoped<IResumeService, ResumeService>();

    return services;
  }
}
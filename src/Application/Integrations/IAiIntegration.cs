using Application.Dto;
using Domain.Entities.Postgres;

namespace Application.Integrations;

public interface IAiIntegration
{
  Task<FetchResumeDto> OptimiseGenericAsync(FetchResumeDto resumeData);

  Task<FetchResumeDto> OptimiseForJobAsync(FetchResumeDto resumeData, List<FetchProjectDto> projects, string jobDescription);
}
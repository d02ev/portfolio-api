using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IExperienceService
{
  Task<CreateResourceResponse<IDictionary<string, string>>> CreateExperience(ExperienceDto experienceDto);

  Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllExperiences();

  Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllDeletedExperiences();

  Task<FetchResourceResponse<FetchExperienceDto>> FetchExperienceById(string experienceId);

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateExperience(string experienceId, UpdateExperienceDto updateExperienceDto);

  Task<DeleteResourceResponse> DeleteExperience(string experienceId);
}
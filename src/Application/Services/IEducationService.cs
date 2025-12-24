using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IEducationService
{
  Task<CreateResourceResponse> CreateEducation(EducationDto educationDto);

  Task<FetchResourceResponse<FetchEducationDto>> FetchEducation();

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateEducation(string educationId, UpdateEducationDto updateEducationDto);
}
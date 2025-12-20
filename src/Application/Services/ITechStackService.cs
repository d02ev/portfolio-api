using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface ITechStackService
{
  Task<CreateResourceResponse> CreateTechStack(TechStackDto techStackDto);

  Task<FetchResourceResponse<FetchTechStackDto>> FetchTechStack();

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateTechStack(string techStackId, UpdateTechStackDto updateTechStackDto);
}
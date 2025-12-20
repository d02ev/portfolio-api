using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IAboutService
{
  Task<CreateResourceResponse> CreateAbout(AboutDto aboutDto);

  Task<FetchResourceResponse<FetchAboutDto>> FetchAbout();

  Task<FetchResourceResponse<FetchAboutDto>> FetchAboutById(string aboutId);

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateAbout(string aboutId, UpdateAboutDto updateAboutDto);
}
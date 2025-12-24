using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Newtonsoft.Json;

namespace Application.Services;

public class EducationService(IEducationRepository educationRepository, IMapper mapper) : IEducationService
{
  private readonly IEducationRepository _educationRepository = educationRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse> CreateEducation(EducationDto educationDto)
  {
    var _ = await _educationRepository.FetchAsync();
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.Education, "Education details already exist.");
    }

    var education = _mapper.Map<Education>(educationDto);
    await _educationRepository.CreateAsync(education);

    return new CreateResourceResponse(ResourceNames.Education);
  }

  public async Task<FetchResourceResponse<FetchEducationDto>> FetchEducation()
  {
    var education = await _educationRepository.FetchAsync() ?? throw new NotFoundException(ResourceNames.Education);
    var fetchEducationDto = _mapper.Map<FetchEducationDto>(education);

    return new FetchResourceResponse<FetchEducationDto>(ResourceNames.Education, fetchEducationDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateEducation(string educationId, UpdateEducationDto updateEducationDto)
  {
    var _ = await _educationRepository.FetchByIdAsync(educationId) ?? throw new NotFoundException(ResourceNames.Education, educationId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject(updateEducationDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    changes["id"] = educationId;

    await _educationRepository.UpdateAsync(educationId, serializedChanges);

    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Education, changes);
  }
}
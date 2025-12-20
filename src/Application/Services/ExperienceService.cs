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

public class ExperienceService(IExperienceRepository experienceRepository, IMapper mapper) : IExperienceService
{
  private readonly IExperienceRepository _experienceRepository = experienceRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse<IDictionary<string, string>>> CreateExperience(ExperienceDto experienceDto)
  {
    var _ = await _experienceRepository.FetchByCompanyNameAndJobTitle(experienceDto.CompanyName, experienceDto.JobTitle);
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.Experience, $"Experience with company name {experienceDto.CompanyName} and job title {experienceDto.JobTitle} already exists.");
    }

    var experience = _mapper.Map<Experience>(experienceDto);
    await _experienceRepository.CreateAsync(experience);

    var newExperience = await _experienceRepository.FetchByCompanyNameAndJobTitle(experienceDto.CompanyName, experienceDto.JobTitle) ?? throw new InternalServerException(ResourceNames.Experience, "An error occurred while creating the experience.");

    return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Experience, new Dictionary<string, string>
    {
      { nameof(newExperience.CompanyName).ToLowerInvariant(), newExperience.CompanyName },
      { nameof(newExperience.JobTitle).ToLowerInvariant(), newExperience.JobTitle }
    });
  }

  public async Task<DeleteResourceResponse> DeleteExperience(string experienceId)
  {
    var _ = await _experienceRepository.FetchByIdAsync(experienceId) ?? throw new NotFoundException(ResourceNames.Experience, experienceId);

    await _experienceRepository.DeleteAsync(experienceId);

    var deletedExperience = await _experienceRepository.FetchByIdAsync(experienceId);
    if (deletedExperience is not null)
    {
      throw new InternalServerException(ResourceNames.Experience, "An error occurred while deleting the experience.");
    }

    return new DeleteResourceResponse(ResourceNames.Experience, experienceId);
  }

  public async Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllDeletedExperiences()
  {
    var deletedExperiences = await _experienceRepository.FetchAllDeletedAsync();
    var fetchExperienceDtos = _mapper.Map<IList<FetchExperienceDto>>(deletedExperiences);

    return new FetchResourceResponse<IList<FetchExperienceDto>>(ResourceNames.Experience, fetchExperienceDtos);
  }

  public async Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllExperiences()
  {
    var experiences = await _experienceRepository.FetchAllAsync();
    var fetchExperienceDtos = _mapper.Map<IList<FetchExperienceDto>>(experiences);

    return new FetchResourceResponse<IList<FetchExperienceDto>>(ResourceNames.Experience, fetchExperienceDtos);
  }

  public async Task<FetchResourceResponse<FetchExperienceDto>> FetchExperienceById(string experienceId)
  {
    var experience = await _experienceRepository.FetchByIdAsync(experienceId) ?? throw new NotFoundException(ResourceNames.Experience, experienceId);
    var fetchExperienceDto = _mapper.Map<FetchExperienceDto>(experience);

    return new FetchResourceResponse<FetchExperienceDto>(ResourceNames.Experience, fetchExperienceDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateExperience(string experienceId, UpdateExperienceDto updateExperienceDto)
  {
    var _ = await _experienceRepository.FetchByIdAsync(experienceId) ?? throw new NotFoundException(ResourceNames.Experience, experienceId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateExperienceDto>(updateExperienceDto);

    // check for 'StartDate' and 'EndDate'
    if (changes.TryGetValue("StartDate", out object? startDate))
    {
      changes["StartDate"] = DateHelper.ConvertMonthYearStringToDate(startDate.ToString()!);
    }
    if (changes.TryGetValue("EndDate", out object? endDate))
    {
      changes["EndDate"] = DateHelper.ConvertMonthYearStringToDate(endDate.ToString()!);
    }

    var serializedChanges = JsonConvert.SerializeObject(changes);

    await _experienceRepository.UpdateAsync(experienceId, serializedChanges);
    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Experience, changes);
  }
}
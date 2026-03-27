using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services;

public class ExperienceService(IExperienceRepository experienceRepository, IMapper mapper, ILogger<ExperienceService> logger) : IExperienceService
{
  private readonly IExperienceRepository _experienceRepository = experienceRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<ExperienceService> _logger = logger;

  public async Task<CreateResourceResponse<IDictionary<string, string>>> CreateExperience(ExperienceDto experienceDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateExperience));
    try
    {
      var existingExperience = await _experienceRepository.FetchByCompanyNameAndJobTitle(experienceDto.CompanyName, experienceDto.JobTitle);
      if (existingExperience is not null)
      {
        _logger.LogWarning("Duplicate experience detected while creating experience.");
        throw new BadRequestException(ResourceNames.Experience, $"Experience with company name {experienceDto.CompanyName} and job title {experienceDto.JobTitle} already exists.");
      }

      var experience = _mapper.Map<Experience>(experienceDto);
      await _experienceRepository.CreateAsync(experience);

      var newExperience = await _experienceRepository.FetchByCompanyNameAndJobTitle(experienceDto.CompanyName, experienceDto.JobTitle);
      if (newExperience is null)
      {
        throw new InternalServerException(ResourceNames.Experience, "An error occurred while creating the experience.");
      }

      _logger.LogInformation("Completed {Operation}. CompanyName={CompanyName}.", nameof(CreateExperience), newExperience.CompanyName);
      return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Experience, new Dictionary<string, string>
      {
        { nameof(newExperience.CompanyName).ToLowerInvariant(), newExperience.CompanyName },
        { nameof(newExperience.JobTitle).ToLowerInvariant(), newExperience.JobTitle }
      });
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(CreateExperience));
      throw;
    }
  }

  public async Task<DeleteResourceResponse> DeleteExperience(string experienceId)
  {
    _logger.LogInformation("Started {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteExperience), experienceId);
    try
    {
      var existingExperience = await _experienceRepository.FetchByIdAsync(experienceId);
      if (existingExperience is null)
      {
        _logger.LogWarning("Experience not found for delete. ExperienceId={ExperienceId}.", experienceId);
        throw new NotFoundException(ResourceNames.Experience, experienceId);
      }

      await _experienceRepository.DeleteAsync(experienceId);

      var deletedExperience = await _experienceRepository.FetchByIdAsync(experienceId);
      if (deletedExperience is not null)
      {
        throw new InternalServerException(ResourceNames.Experience, "An error occurred while deleting the experience.");
      }

      _logger.LogInformation("Completed {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteExperience), experienceId);
      return new DeleteResourceResponse(ResourceNames.Experience, experienceId);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteExperience), experienceId);
      throw;
    }
  }

  public async Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllDeletedExperiences()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchAllDeletedExperiences));
    try
    {
      var deletedExperiences = await _experienceRepository.FetchAllDeletedAsync();
      var fetchExperienceDtos = _mapper.Map<IList<FetchExperienceDto>>(deletedExperiences);

      _logger.LogInformation("Completed {Operation}. Count={Count}.", nameof(FetchAllDeletedExperiences), fetchExperienceDtos.Count);
      return new FetchResourceResponse<IList<FetchExperienceDto>>(ResourceNames.Experience, fetchExperienceDtos);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchAllDeletedExperiences));
      throw;
    }
  }

  public async Task<FetchResourceResponse<IList<FetchExperienceDto>>> FetchAllExperiences()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchAllExperiences));
    try
    {
      var experiences = await _experienceRepository.FetchAllAsync();
      var fetchExperienceDtos = _mapper.Map<IList<FetchExperienceDto>>(experiences);

      _logger.LogInformation("Completed {Operation}. Count={Count}.", nameof(FetchAllExperiences), fetchExperienceDtos.Count);
      return new FetchResourceResponse<IList<FetchExperienceDto>>(ResourceNames.Experience, fetchExperienceDtos);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchAllExperiences));
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchExperienceDto>> FetchExperienceById(string experienceId)
  {
    _logger.LogInformation("Started {Operation} for ExperienceId={ExperienceId}.", nameof(FetchExperienceById), experienceId);
    try
    {
      var experience = await _experienceRepository.FetchByIdAsync(experienceId);
      if (experience is null)
      {
        _logger.LogWarning("Experience not found for ExperienceId={ExperienceId}.", experienceId);
        throw new NotFoundException(ResourceNames.Experience, experienceId);
      }

      var fetchExperienceDto = _mapper.Map<FetchExperienceDto>(experience);
      _logger.LogInformation("Completed {Operation} for ExperienceId={ExperienceId}.", nameof(FetchExperienceById), experienceId);
      return new FetchResourceResponse<FetchExperienceDto>(ResourceNames.Experience, fetchExperienceDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ExperienceId={ExperienceId}.", nameof(FetchExperienceById), experienceId);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateExperience(string experienceId, UpdateExperienceDto updateExperienceDto)
  {
    _logger.LogInformation("Started {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateExperience), experienceId);
    try
    {
      var existingExperience = await _experienceRepository.FetchByIdAsync(experienceId);
      if (existingExperience is null)
      {
        _logger.LogWarning("Experience not found for update. ExperienceId={ExperienceId}.", experienceId);
        throw new NotFoundException(ResourceNames.Experience, experienceId);
      }

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

      _logger.LogInformation("Completed {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateExperience), experienceId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Experience, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateExperience), experienceId);
      throw;
    }
  }
}

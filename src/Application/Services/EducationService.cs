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

public class EducationService(IEducationRepository educationRepository, IMapper mapper, ILogger<EducationService> logger) : IEducationService
{
  private readonly IEducationRepository _educationRepository = educationRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<EducationService> _logger = logger;

  public async Task<CreateResourceResponse> CreateEducation(EducationDto educationDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateEducation));

    try
    {
      var existingEducation = await _educationRepository.FetchAsync();
      if (existingEducation is not null)
      {
        _logger.LogWarning("Duplicate education detected while creating education.");
        throw new BadRequestException(ResourceNames.Education, "Education details already exist.");
      }

      var education = _mapper.Map<Education>(educationDto);
      await _educationRepository.CreateAsync(education);

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(CreateEducation), ResourceNames.Education);
      return new CreateResourceResponse(ResourceNames.Education);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(CreateEducation), ResourceNames.Education);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchEducationDto>> FetchEducation()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchEducation));

    try
    {
      var education = await _educationRepository.FetchAsync();
      if (education is null)
      {
        _logger.LogWarning("Education not found.");
        throw new NotFoundException(ResourceNames.Education);
      }

      var fetchEducationDto = _mapper.Map<FetchEducationDto>(education);
      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(FetchEducation), ResourceNames.Education);
      return new FetchResourceResponse<FetchEducationDto>(ResourceNames.Education, fetchEducationDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(FetchEducation), ResourceNames.Education);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateEducation(string educationId, UpdateEducationDto updateEducationDto)
  {
    _logger.LogInformation("Started {Operation} for EducationId={EducationId}.", nameof(UpdateEducation), educationId);

    try
    {
      var existingEducation = await _educationRepository.FetchByIdAsync(educationId);
      if (existingEducation is null)
      {
        _logger.LogWarning("Education not found for update. EducationId={EducationId}.", educationId);
        throw new NotFoundException(ResourceNames.Education, educationId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject(updateEducationDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      changes["id"] = educationId;
      await _educationRepository.UpdateAsync(educationId, serializedChanges);

      _logger.LogInformation("Completed {Operation} for EducationId={EducationId}.", nameof(UpdateEducation), educationId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Education, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for EducationId={EducationId}.", nameof(UpdateEducation), educationId);
      throw;
    }
  }
}

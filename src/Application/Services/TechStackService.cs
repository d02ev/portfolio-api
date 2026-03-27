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

public class TechStackService(ITechStackRepository techStackRepository, IMapper mapper, ILogger<TechStackService> logger) : ITechStackService
{
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<TechStackService> _logger = logger;

  public async Task<CreateResourceResponse> CreateTechStack(TechStackDto techStackDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateTechStack));

    try
    {
      var existingTechStack = await _techStackRepository.FetchAsync();
      if (existingTechStack is not null)
      {
        _logger.LogWarning("Duplicate tech stack detected while creating tech stack.");
        throw new BadRequestException(ResourceNames.TechStack, "Tech stack already exists.");
      }

      var techStack = _mapper.Map<TechStack>(techStackDto);
      await _techStackRepository.CreateAsync(techStack);

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(CreateTechStack), ResourceNames.TechStack);
      return new CreateResourceResponse(ResourceNames.TechStack);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(CreateTechStack), ResourceNames.TechStack);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchTechStackDto>> FetchTechStack()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchTechStack));

    try
    {
      var techStack = await _techStackRepository.FetchAsync();
      if (techStack is null)
      {
        _logger.LogWarning("TechStack not found.");
        throw new NotFoundException(ResourceNames.TechStack);
      }

      var fetchTechStackDto = _mapper.Map<FetchTechStackDto>(techStack);
      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(FetchTechStack), ResourceNames.TechStack);
      return new FetchResourceResponse<FetchTechStackDto>(ResourceNames.TechStack, fetchTechStackDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(FetchTechStack), ResourceNames.TechStack);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateTechStack(string techStackId, UpdateTechStackDto updateTechStackDto)
  {
    _logger.LogInformation("Started {Operation} for TechStackId={TechStackId}.", nameof(UpdateTechStack), techStackId);

    try
    {
      var existingTechStack = await _techStackRepository.FetchByIdAsync(techStackId);
      if (existingTechStack is null)
      {
        _logger.LogWarning("TechStack not found for update. TechStackId={TechStackId}.", techStackId);
        throw new NotFoundException(ResourceNames.TechStack, techStackId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateTechStackDto>(updateTechStackDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      changes["id"] = techStackId;
      await _techStackRepository.UpdateAsync(techStackId, serializedChanges);

      _logger.LogInformation("Completed {Operation} for TechStackId={TechStackId}.", nameof(UpdateTechStack), techStackId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.TechStack, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for TechStackId={TechStackId}.", nameof(UpdateTechStack), techStackId);
      throw;
    }
  }
}

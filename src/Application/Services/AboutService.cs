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

public class AboutService(IAboutRepository aboutRepository, ITechStackRepository techStackRepository, IMapper mapper, ILogger<AboutService> logger) : IAboutService
{
  private readonly IAboutRepository _aboutRepository = aboutRepository;
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<AboutService> _logger = logger;

  public async Task<CreateResourceResponse> CreateAbout(AboutDto aboutDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateAbout));

    try
    {
      var existingAbout = await _aboutRepository.FetchByNameAsync(aboutDto.Bio.Name);
      if (existingAbout is not null)
      {
        _logger.LogWarning("Duplicate about detected while creating about. ResourceName={ResourceName}.", ResourceNames.About);
        throw new BadRequestException(ResourceNames.About, $"About with name {aboutDto.Bio.Name} already exists.");
      }

      var about = _mapper.Map<About>(aboutDto);
      await _aboutRepository.CreateAsync(about);

      var createdAbout = await _aboutRepository.FetchByNameAsync(aboutDto.Bio.Name);
      if (createdAbout is null)
      {
        throw new InternalServerException(ResourceNames.About, "An error occurred while creating about.");
      }

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(CreateAbout), ResourceNames.About);
      return new CreateResourceResponse(ResourceNames.About);
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
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(CreateAbout), ResourceNames.About);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchAboutDto>> FetchAbout()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchAbout));

    try
    {
      var about = await _aboutRepository.FetchAsync();
      if (about is null)
      {
        _logger.LogWarning("About not found. ResourceName={ResourceName}.", ResourceNames.About);
        throw new NotFoundException(ResourceNames.About);
      }

      var techStack = await _techStackRepository.FetchByIdAsync(about.TechStackId);
      var fetchAboutDto = _mapper.Map<FetchAboutDto>(about);
      fetchAboutDto.TechStack = _mapper.Map<FetchTechStackDto>(techStack);

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(FetchAbout), ResourceNames.About);
      return new FetchResourceResponse<FetchAboutDto>(ResourceNames.About, fetchAboutDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(FetchAbout), ResourceNames.About);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchAboutDto>> FetchAboutById(string aboutId)
  {
    _logger.LogInformation("Started {Operation} for AboutId={AboutId}.", nameof(FetchAboutById), aboutId);

    try
    {
      var about = await _aboutRepository.FetchByIdAsync(aboutId);
      if (about is null)
      {
        _logger.LogWarning("About not found for AboutId={AboutId}.", aboutId);
        throw new NotFoundException(ResourceNames.About, aboutId);
      }

      var techStack = await _techStackRepository.FetchByIdAsync(about.TechStackId);
      var fetchAboutDto = _mapper.Map<FetchAboutDto>(about);
      fetchAboutDto.TechStack = _mapper.Map<FetchTechStackDto>(techStack);

      _logger.LogInformation("Completed {Operation} for AboutId={AboutId}.", nameof(FetchAboutById), aboutId);
      return new FetchResourceResponse<FetchAboutDto>(ResourceNames.About, fetchAboutDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for AboutId={AboutId}.", nameof(FetchAboutById), aboutId);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateAbout(string aboutId, UpdateAboutDto updateAboutDto)
  {
    _logger.LogInformation("Started {Operation} for AboutId={AboutId}.", nameof(UpdateAbout), aboutId);

    try
    {
      var existingAbout = await _aboutRepository.FetchByIdAsync(aboutId);
      if (existingAbout is null)
      {
        _logger.LogWarning("About not found for update. AboutId={AboutId}.", aboutId);
        throw new NotFoundException(ResourceNames.About, aboutId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateAboutDto>(updateAboutDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      await _aboutRepository.UpdateAsync(aboutId, serializedChanges);

      _logger.LogInformation("Completed {Operation} for AboutId={AboutId}.", nameof(UpdateAbout), aboutId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.About, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for AboutId={AboutId}.", nameof(UpdateAbout), aboutId);
      throw;
    }
  }
}

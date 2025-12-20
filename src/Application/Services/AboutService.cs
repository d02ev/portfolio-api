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

public class AboutService(IAboutRepository aboutRepository, ITechStackRepository techStackRepository, IMapper mapper) : IAboutService
{
  private readonly IAboutRepository _aboutRepository = aboutRepository;
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse> CreateAbout(AboutDto aboutDto)
  {
    var _ = await _aboutRepository.FetchByNameAsync(aboutDto.Bio.Name);
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.About, $"About with name {aboutDto.Bio.Name} already exists.");
    }

    var about = _mapper.Map<About>(aboutDto);
    await _aboutRepository.CreateAsync(about);

    var createdAbout = await _aboutRepository.FetchByNameAsync(aboutDto.Bio.Name) ?? throw new InternalServerException(ResourceNames.About, "An error occurred while creating about.");

    return new CreateResourceResponse(ResourceNames.About);
  }

  public async Task<FetchResourceResponse<FetchAboutDto>> FetchAbout()
  {
    var about = await _aboutRepository.FetchAsync() ?? throw new NotFoundException(ResourceNames.About);
    var techStack = await _techStackRepository.FetchByIdAsync(about.TechStackId);
    var fetchAboutDto = _mapper.Map<FetchAboutDto>(about);
    fetchAboutDto.TechStack = _mapper.Map<FetchTechStackDto>(techStack);

    return new FetchResourceResponse<FetchAboutDto>(ResourceNames.About, fetchAboutDto);
  }

  public async Task<FetchResourceResponse<FetchAboutDto>> FetchAboutById(string aboutId)
  {
    var about = await _aboutRepository.FetchByIdAsync(aboutId) ?? throw new NotFoundException(ResourceNames.About, aboutId);
    var techStack = await _techStackRepository.FetchByIdAsync(about.TechStackId);
    var fetchAboutDto = _mapper.Map<FetchAboutDto>(about);
    fetchAboutDto.TechStack = _mapper.Map<FetchTechStackDto>(techStack);

    return new FetchResourceResponse<FetchAboutDto>(ResourceNames.About, fetchAboutDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateAbout(string aboutId, UpdateAboutDto updateAboutDto)
  {
    var _ = await _aboutRepository.FetchByIdAsync(aboutId) ?? throw new NotFoundException(ResourceNames.About, aboutId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateAboutDto>(updateAboutDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    await _aboutRepository.UpdateAsync(aboutId, serializedChanges);

    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.About, changes);
  }
}
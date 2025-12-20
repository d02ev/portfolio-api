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

public class TechStackService(ITechStackRepository techStackRepository, IMapper mapper) : ITechStackService
{
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse> CreateTechStack(TechStackDto techStackDto)
  {
    var _ = await _techStackRepository.FetchAsync();
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.TechStack, "Tech stack already exists.");
    }

    var techStack = _mapper.Map<TechStack>(techStackDto);
    await _techStackRepository.CreateAsync(techStack);

    return new CreateResourceResponse(ResourceNames.TechStack);
  }

  public async Task<FetchResourceResponse<FetchTechStackDto>> FetchTechStack()
  {
    var techStack = await _techStackRepository.FetchAsync() ?? throw new NotFoundException(ResourceNames.TechStack);
    var fetchTechStackDto = _mapper.Map<FetchTechStackDto>(techStack);

    return new FetchResourceResponse<FetchTechStackDto>(ResourceNames.TechStack, fetchTechStackDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateTechStack(string techStackId, UpdateTechStackDto updateTechStackDto)
  {
    var _ = await _techStackRepository.FetchByIdAsync(techStackId) ?? throw new NotFoundException(ResourceNames.TechStack, techStackId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateTechStackDto>(updateTechStackDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    changes["id"] = techStackId;

    await _techStackRepository.UpdateAsync(techStackId, serializedChanges);

    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.TechStack, changes);
  }
}
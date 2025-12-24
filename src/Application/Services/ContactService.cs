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

public class ContactService(IContactRepository contactRepository, IMapper mapper) : IContactService
{
  private readonly IContactRepository _contactRepository = contactRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse> CreateContact(ContactDto contactDto)
  {
    var _ = await _contactRepository.FetchAsync();
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.Contact, "Contact details already exists.");
    }

    var contact = _mapper.Map<Contact>(contactDto);
    await _contactRepository.CreateAsync(contact);

    return new CreateResourceResponse(ResourceNames.Contact);
  }

  public async Task<FetchResourceResponse<FetchContactDto>> FetchContact()
  {
    var contact = await _contactRepository.FetchAsync() ?? throw new NotFoundException(ResourceNames.Contact);
    var fetchContactDto = _mapper.Map<FetchContactDto>(contact);

    return new FetchResourceResponse<FetchContactDto>(ResourceNames.Contact, fetchContactDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateContact(string contactId, UpdateContactDto updateContactDto)
  {
    var _ = await _contactRepository.FetchByIdAsync(contactId) ?? throw new NotFoundException(ResourceNames.Contact, contactId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateContactDto>(updateContactDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    changes["id"] = contactId;

    await _contactRepository.UpdateAsync(contactId, serializedChanges);

    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Contact, changes);
  }
}
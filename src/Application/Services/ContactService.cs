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

public class ContactService(IContactRepository contactRepository, IMapper mapper, ILogger<ContactService> logger) : IContactService
{
  private readonly IContactRepository _contactRepository = contactRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<ContactService> _logger = logger;

  public async Task<CreateResourceResponse> CreateContact(ContactDto contactDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateContact));

    try
    {
      var existingContact = await _contactRepository.FetchAsync();
      if (existingContact is not null)
      {
        _logger.LogWarning("Duplicate contact detected while creating contact.");
        throw new BadRequestException(ResourceNames.Contact, "Contact details already exists.");
      }

      var contact = _mapper.Map<Contact>(contactDto);
      await _contactRepository.CreateAsync(contact);

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(CreateContact), ResourceNames.Contact);
      return new CreateResourceResponse(ResourceNames.Contact);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(CreateContact), ResourceNames.Contact);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchContactDto>> FetchContact()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchContact));

    try
    {
      var contact = await _contactRepository.FetchAsync();
      if (contact is null)
      {
        _logger.LogWarning("Contact not found.");
        throw new NotFoundException(ResourceNames.Contact);
      }

      var fetchContactDto = _mapper.Map<FetchContactDto>(contact);
      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(FetchContact), ResourceNames.Contact);
      return new FetchResourceResponse<FetchContactDto>(ResourceNames.Contact, fetchContactDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResourceName={ResourceName}.", nameof(FetchContact), ResourceNames.Contact);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateContact(string contactId, UpdateContactDto updateContactDto)
  {
    _logger.LogInformation("Started {Operation} for ContactId={ContactId}.", nameof(UpdateContact), contactId);

    try
    {
      var existingContact = await _contactRepository.FetchByIdAsync(contactId);
      if (existingContact is null)
      {
        _logger.LogWarning("Contact not found for update. ContactId={ContactId}.", contactId);
        throw new NotFoundException(ResourceNames.Contact, contactId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateContactDto>(updateContactDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      changes["id"] = contactId;
      await _contactRepository.UpdateAsync(contactId, serializedChanges);

      _logger.LogInformation("Completed {Operation} for ContactId={ContactId}.", nameof(UpdateContact), contactId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Contact, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ContactId={ContactId}.", nameof(UpdateContact), contactId);
      throw;
    }
  }
}

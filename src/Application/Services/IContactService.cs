using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IContactService
{
  Task<CreateResourceResponse> CreateContact(ContactDto contactDto);

  Task<FetchResourceResponse<FetchContactDto>> FetchContact();

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateContact(string contactId, UpdateContactDto updateContactDto);
}
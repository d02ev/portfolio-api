using System.Net;
using Application.Common;
using Application.Dto;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace PorfolioApi.Tests;

public class ContactServiceTests
{
  private readonly Mock<IContactRepository> _contactRepositoryMock;
  private readonly IContactService _contactService;

  public ContactServiceTests()
  {
    _contactRepositoryMock = new Mock<IContactRepository>();
    _contactService = new ContactService(_contactRepositoryMock.Object, TestMapperFactory.Create());
  }

  [Fact]
  public async Task CreateContact_ShouldCreateContact_WhenContactDoesNotExist()
  {
    var contactDto = new ContactDto
    {
      Email = "vikram@example.com",
      Mobile = "1234567890",
      Github = "github.com/vikram",
      Linkedin = "linkedin.com/in/vikram",
      Website = "vikram.dev"
    };

    _contactRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync((Contact)null!);

    var response = await _contactService.CreateContact(contactDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResourceName.Should().Be(ResourceNames.Contact);
    _contactRepositoryMock.Verify(r => r.CreateAsync(It.Is<Contact>(c =>
      c.Email == contactDto.Email &&
      c.Website == contactDto.Website)), Times.Once);
  }

  [Fact]
  public async Task CreateContact_ShouldThrowBadRequestException_WhenContactAlreadyExists()
  {
    _contactRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync(new Contact { Id = "contact-1" });

    Func<Task> act = () => _contactService.CreateContact(new ContactDto());

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.Contact &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task FetchContact_ShouldThrowNotFoundException_WhenContactDoesNotExist()
  {
    _contactRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync((Contact)null!);

    Func<Task> act = () => _contactService.FetchContact();

    await act.Should()
      .ThrowAsync<NotFoundException>()
      .Where(ex => ex.ResourceName == ResourceNames.Contact &&
                   ex.StatusCode == (int)HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateContact_ShouldReturnUpdatedFieldsAndIncludeId_WhenContactExists()
  {
    _contactRepositoryMock.Setup(r => r.FetchByIdAsync("contact-1"))
      .ReturnsAsync(new Contact { Id = "contact-1" });

    var response = await _contactService.UpdateContact("contact-1", new UpdateContactDto
    {
      Email = "updated@example.com"
    });

    response.Data.Should().ContainKey("id").WhoseValue.Should().Be("contact-1");
    response.Data.Should().ContainKey("Email").WhoseValue.Should().Be("updated@example.com");
    _contactRepositoryMock.Verify(r => r.UpdateAsync("contact-1", It.Is<string>(s => s.Contains("updated@example.com"))), Times.Once);
  }
}

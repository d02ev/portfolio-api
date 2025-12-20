using System.Net;

namespace Domain.Exceptions;

public class BadRequestException(string resourceName, string message) : Exception(message)
{
  public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;

  public string ResourceName { get; set; } = resourceName;
}
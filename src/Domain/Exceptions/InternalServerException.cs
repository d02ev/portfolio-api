using System.Net;

namespace Domain.Exceptions;

public class InternalServerException(string resourceName, string message) : Exception(message)
{
  public int StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;

  public string ResourceName { get; set; } = resourceName;
}
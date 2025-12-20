using System.Net;

namespace Domain.Exceptions;

public class NotFoundException(string resourceName, object? key = null) : Exception(key is not null ? $"{resourceName} with key '{key}' not found." : $"{resourceName} not found.")
{
  public int StatusCode { get; set; } = (int)HttpStatusCode.NotFound;

  public string ResourceName { get; set; } = resourceName;
} 
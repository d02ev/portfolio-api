using System.Net;

namespace Domain.Exceptions;

public class JobFailureException : Exception
{
  public JobFailureException(string message = "The workflow run failed due to some error.") 
    : base(message)
  {
  }

  public int StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;

  public string ResourceName { get; set; } = "WorkflowRun";
}
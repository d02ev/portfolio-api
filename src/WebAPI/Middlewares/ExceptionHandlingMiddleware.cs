
using System.Net;
using Application.Common;
using Application.Responses;
using Domain.Exceptions;
using MongoDB.Driver;

namespace WebAPI.Middlewares;

public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    try
    {
      await next(context);
    }
    catch (MongoException mEx)
    {
      _logger.LogError("DB operation failed: {ErrorMessage}. Inner exception: {InnerException}",
        mEx.Message,
        mEx.InnerException?.Message);
      await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError, ResponseCodes.DbError, ResourceNames.Db, mEx.Message);
    }
    catch (BadRequestException bRex)
    {
      await HandleExceptionAsync(context, bRex.StatusCode, ResponseCodes.BadRequest, bRex.ResourceName, bRex.Message);
    }
    catch (NotFoundException nFex)
    {
      await HandleExceptionAsync(context, nFex.StatusCode, ResponseCodes.ResourceNotFound, nFex.ResourceName, nFex.Message);
    }
    catch (JobFailureException jFex)
    {
      await HandleExceptionAsync(context, jFex.StatusCode, ResponseCodes.JobFailure, jFex.ResourceName, jFex.Message);
    }
    catch (InternalServerException iEx)
    {
      _logger.LogError("Internal error: {ErrorMessage}. Inner exception: {InnerException}", iEx.Message, iEx.InnerException);
      await HandleExceptionAsync(context, iEx.StatusCode, ResponseCodes.InternalServerError, iEx.ResourceName, iEx.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError("Unknown error: {ErrorMessage}. Inner exception: {InnerException}",
        ex.Message,
        ex.InnerException);
      await HandleExceptionAsync(context);
    }
  }

  private static Task HandleExceptionAsync(HttpContext context, int statusCode = (int)HttpStatusCode.InternalServerError, string responseCode = ResponseCodes.InternalServerError, string resourceName = ResourceNames.App, string message = "Something went wrong. Please try again later.")
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = statusCode;

    var errorResponse = new ErrorResponse
    {
      StatusCode = statusCode,
      ResponseCode = responseCode,
      ResourceName = resourceName,
      Message = message,
    };

    return context.Response.WriteAsJsonAsync(errorResponse);
  }
}

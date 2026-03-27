using System.Net;
using System.Diagnostics;
using Application.Common;
using Application.Responses;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace WebAPI.Middlewares;

public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    var traceId = context.TraceIdentifier;
    var requestPath = context.Request.Path.Value ?? string.Empty;
    var httpMethod = context.Request.Method;
    var stopwatch = Stopwatch.StartNew();

    using var scope = _logger.BeginScope(new Dictionary<string, object?>
    {
      ["TraceId"] = traceId,
      ["RequestPath"] = requestPath,
      ["HttpMethod"] = httpMethod,
    });

    _logger.LogInformation("Handling request {HttpMethod} {RequestPath}.", httpMethod, requestPath);

    try
    {
      await next(context);
    }
    catch (MongoException mEx)
    {
      _logger.LogError(
        mEx,
        "Database operation failed. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        ResourceNames.Db,
        ResponseCodes.DbError);
      await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError, ResponseCodes.DbError, ResourceNames.Db, mEx.Message);
    }
    catch (BadRequestException bRex)
    {
      _logger.LogWarning(
        bRex,
        "Bad request encountered. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        bRex.ResourceName,
        ResponseCodes.BadRequest);
      await HandleExceptionAsync(context, bRex.StatusCode, ResponseCodes.BadRequest, bRex.ResourceName, bRex.Message);
    }
    catch (NotFoundException nFex)
    {
      _logger.LogWarning(
        nFex,
        "Resource not found. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        nFex.ResourceName,
        ResponseCodes.ResourceNotFound);
      await HandleExceptionAsync(context, nFex.StatusCode, ResponseCodes.ResourceNotFound, nFex.ResourceName, nFex.Message);
    }
    catch (JobFailureException jFex)
    {
      _logger.LogError(
        jFex,
        "Workflow job failed. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        jFex.ResourceName,
        ResponseCodes.JobFailure);
      await HandleExceptionAsync(context, jFex.StatusCode, ResponseCodes.JobFailure, jFex.ResourceName, jFex.Message);
    }
    catch (InternalServerException iEx)
    {
      _logger.LogError(
        iEx,
        "Internal server exception occurred. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        iEx.ResourceName,
        ResponseCodes.InternalServerError);
      await HandleExceptionAsync(context, iEx.StatusCode, ResponseCodes.InternalServerError, iEx.ResourceName, iEx.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Unhandled exception occurred. ResourceName={ResourceName}, ResponseCode={ResponseCode}",
        ResourceNames.App,
        ResponseCodes.InternalServerError);
      await HandleExceptionAsync(context);
    }
    finally
    {
      stopwatch.Stop();
      _logger.LogInformation(
        "Completed request {HttpMethod} {RequestPath} with status code {StatusCode} in {ElapsedMilliseconds}ms.",
        httpMethod,
        requestPath,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
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

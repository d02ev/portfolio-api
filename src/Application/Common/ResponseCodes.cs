namespace Application.Common;

public static class ResponseCodes
{
  public const string ResourceCreated = "RESOURCE_CREATED";

  public const string ResourceUpdated = "RESOURCE_UPDATED";

  public const string ResourceDeleted = "RESOURCE_DELETED";

  public const string ResourceFetched = "RESOURCE_FETCHED";

  public const string ResourceNotFound = "RESOURCE_NOT_FOUND";

  public const string UserLogin = "USER_LOGIN";

  public const string UserLogout = "USER_LOGOUT";

  public const string TokenRefresh = "TOKEN_REFRESH";

  public const string BadRequest = "BAD_REQUEST";

  public const string InternalServerError = "INTERNAL_SERVER_ERROR";

  public const string Unauthorised = "UNAUTHORISED";

  public const string InvalidToken = "INVALID_TOKEN";

  public const string DbError = "DB_ERROR";

  public const string JobFailure = "JOB_FAILURE";
}
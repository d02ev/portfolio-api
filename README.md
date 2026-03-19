# Portfolio API

Portfolio API is a layered ASP.NET Core Web API for managing portfolio content and generating resumes. It stores portfolio data in MongoDB, secures admin operations with JWT bearer auth, and integrates with Supabase, GitHub, AI services, and Telegram to generate and track resume builds.

## What It Does

- Manages portfolio resources such as `about`, `projects`, `experience`, `education`, `contact`, `tech stack`, and `resume`
- Provides authentication endpoints for registering, logging in, refreshing tokens, and logging out
- Generates generic or job-tailored resumes from stored data and external templates
- Pushes generated LaTeX files to a GitHub repository and triggers a `compile.yml` workflow
- Tracks resume build status through Supabase and sends Telegram notifications for workflow events

## Tech Stack

- .NET 9 / ASP.NET Core Web API
- MongoDB for portfolio data
- Supabase for file storage and resume job tracking
- GitHub Actions for resume compilation workflows
- Telegram bot integration for notifications
- JWT bearer authentication
- AutoMapper, Newtonsoft.Json, RazorLight
- xUnit, Moq, FluentAssertions for tests

## Solution Structure

```text
src/
  Application/     Business logic, DTOs, interfaces, service layer
  Domain/          Entities, enums, exceptions, configuration models
  Infrastructure/  Mongo repositories, external integrations, auth helpers
  WebAPI/          Controllers, startup, middleware, HTTP surface
test/
  PorfolioApi.Tests/  Unit tests
```

## Architecture Notes

The app follows a straightforward layered architecture:

- `WebAPI` handles HTTP routing, auth, serialization, CORS, and middleware
- `Application` contains use-case logic and orchestrates repositories/integrations
- `Infrastructure` provides MongoDB repositories plus GitHub, Supabase, Telegram, and AI integrations
- `Domain` holds shared models and configuration contracts

## Prerequisites

Before running locally, make sure you have:

- .NET 9 SDK
- A MongoDB instance
- A valid `appsettings.json` for local development
- Optional external services, depending on which features you want to use:
  - GitHub personal access token and repository with a `compile.yml` workflow
  - Supabase project and storage bucket
  - Telegram bot token and chat ID
  - AI API credentials

## Configuration

The project includes a sample config at [`src/WebAPI/appsettings.Sample.json`](src/WebAPI/appsettings.Sample.json).

Create a local config file:

```bash
cp src/WebAPI/appsettings.Sample.json src/WebAPI/appsettings.json
```

Then fill in the required values:

### Core Settings

- `CorsSettings`
  - `Methods`: allowed HTTP methods
- `MongoDbSettings`
  - `Url`: MongoDB connection string
  - `Db`: database name
- `JwtSettings`
  - `AccessTokenSecretKey`
  - `RefreshTokenSecretKey`
  - `AccessTokenExpiryInMinutes`
  - `RefreshTokenExpiryInDays`

### External Integrations

- `GithubSettings`
  - `PersonalAccessToken`
  - `Owner`
  - `Repo`
  - `WorkflowFilePath`
- `AiSettings`
  - `PersonalAccessToken`
  - `Url`
  - `Model`
  - `GenericOptimisationPasses`
  - `JobOptimisationPasses`
- `TelegramSettings`
  - `BotToken`
  - `ChatId`
- `SupabaseSettings`
  - `ProjectUrl`
  - `ServiceRoleKey`
  - `AnonKey`
  - `Bucket`

### Production Secret File

On startup, the app also tries to load:

```text
/etc/secrets/appsettings.Production.json
```

That file is optional, but it is useful for container and hosted deployments where secrets are mounted at runtime.

## Running Locally

Restore dependencies:

```bash
dotnet restore PortfolioApi.sln
```

Start the API:

```bash
dotnet run --project src/WebAPI/WebAPI.csproj --launch-profile dev
```

The development launch profile binds the API to:

```text
http://localhost:5000
```

## OpenAPI

OpenAPI is only mapped in development. After starting the app in development mode, the spec is available at:

```text
http://localhost:5000/openapi/v1.json
```

## Running Tests

```bash
dotnet test PortfolioApi.sln
```

## Docker

Build the image:

```bash
docker build -t portfolio-api .
```

Run the container:

```bash
docker run --rm -p 8080:8080 portfolio-api
```

The container:

- uses the .NET 9 runtime
- listens on port `8080`
- sets `ASPNETCORE_ENVIRONMENT=Production`
- optionally reads `/etc/secrets/appsettings.Production.json`

## Deployment

The repository includes GitHub Actions workflows under [`.github/workflows`](.github/workflows):

- [`build.yml`](.github/workflows/build.yml) runs restore, build, and test for pushes to `develop` and PRs targeting `master`
- [`deploy-render.yml`](.github/workflows/deploy-render.yml) validates, deploys to Render on pushes to `master`, and checks the health endpoint afterward

## Authentication

JWT bearer authentication is enabled globally for most resource controllers.

- Public endpoints are explicitly marked with `AllowAnonymous`
- Protected endpoints expect `Authorization: Bearer <access-token>`
- Access and refresh tokens are created through the auth endpoints

Example login request:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

## API Overview

### Health

- `GET /api/health` - service health check

### Auth

- `POST /api/auth/register` - register a user
- `POST /api/auth/login` - login and receive access/refresh tokens
- `POST /api/auth/refresh/{token}` - issue a new access token
- `POST /api/auth/logout` - authenticated logout response

### About

- `GET /api/about` - public fetch
- `POST /api/about` - create
- `GET /api/about/{id}` - fetch by id
- `PATCH /api/about/update/{id}` - update

### Project

- `GET /api/project` - public fetch all
- `POST /api/project` - create
- `GET /api/project/deleted` - fetch deleted records
- `GET /api/project/{id}` - fetch by id
- `PATCH /api/project/update/{id}` - update
- `PATCH /api/project/delete/{id}` - soft delete

### Experience

- `GET /api/experience` - public fetch all
- `POST /api/experience` - create
- `GET /api/experience/deleted` - fetch deleted records
- `GET /api/experience/{id}` - fetch by id
- `PATCH /api/experience/update/{id}` - update
- `PATCH /api/experience/delete/{id}` - soft delete

### Education

- `GET /api/education` - fetch all
- `POST /api/education` - create
- `PATCH /api/education/update/{id}` - update

### Contact

- `GET /api/contact` - fetch all
- `POST /api/contact` - create
- `PATCH /api/contact/update/{id}` - update

### Tech Stack

- `GET /api/techstack` - fetch all
- `POST /api/techstack` - create
- `PATCH /api/techstack/update/{id}` - update

### Resume

- `GET /api/resume/latest` - public fetch of latest generated PDF URL
- `GET /api/resume` - fetch current resume definition
- `POST /api/resume` - create a resume definition
- `PATCH /api/resume/update/{id}` - update a resume definition
- `POST /api/resume/generate/generic` - generate a generic resume
- `POST /api/resume/generate/jd` - generate a resume from job description form-data
- `GET /api/resume/status/{jobId}` - fetch async generation status

### Notification

- `POST /api/notification/send` - send Telegram workflow notification

## Resume Generation Flow

Resume generation is one of the main workflows in this API:

1. The API receives resume data plus a template reference.
2. The AI integration optimizes the resume content.
3. The template is downloaded from Supabase and rendered to LaTeX with RazorLight.
4. The generated `.tex` file is pushed to the configured GitHub repository.
5. A GitHub Actions workflow named `compile.yml` is dispatched.
6. Job status is recorded in Supabase.
7. Telegram notifications are sent as the workflow progresses.

For job-specific resume generation, the API also accepts a job description file upload and uses project data to tailor the output.

## Development Notes

- CORS currently allows any origin and any header, while restricting methods based on configuration
- The app uses a custom exception-handling middleware for API error responses
- OpenAPI is exposed only in development
- The repository currently contains unit tests for the service layer

## Useful Commands

```bash
dotnet restore PortfolioApi.sln
dotnet build PortfolioApi.sln
dotnet test PortfolioApi.sln
dotnet run --project src/WebAPI/WebAPI.csproj --launch-profile dev
docker build -t portfolio-api .
```

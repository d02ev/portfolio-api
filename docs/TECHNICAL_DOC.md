# Portfolio API Technical Documentation

`portfolio-api` is a layered ASP.NET Core Web API that manages portfolio content and orchestrates the resume-generation pipeline.

## Responsibilities

The service is responsible for:

- serving portfolio content used by the public website
- handling authenticated admin operations for portfolio resources
- generating generic and job-specific resumes
- pushing generated `.tex` files into the LaTeX compilation workflow
- tracking resume job state through Supabase
- relaying notification requests to Telegram through the internal notification service

## Architecture

The solution follows a layered structure:

```text
src/
  Application/     Use-case logic, DTOs, interfaces, responses, mapping
  Domain/          Entities, enums, exceptions, configuration models
  Infrastructure/  Mongo repositories, external integrations, auth helpers
  WebAPI/          Controllers, startup, middleware, HTTP entrypoints
test/
  PorfolioApi.Tests/  Unit tests
```

Runtime responsibilities by layer:

- `WebAPI`: routing, auth, CORS, controller surface, JSON serialization, exception middleware
- `Application`: orchestration logic for CRUD operations, resume generation, and notifications
- `Infrastructure`: MongoDB access plus GitHub, Supabase, AI, and Telegram integrations
- `Domain`: shared entities, settings contracts, and exception types

## Core Runtime Flows

### Public portfolio content

The public site reads content from anonymous endpoints including:

- `GET /api/about`
- `GET /api/project`
- `GET /api/experience`
- `GET /api/resume/latest`

These endpoints return portfolio content without requiring JWT authentication.

### Resume generation

The resume pipeline is coordinated in `ResumeService`:

1. Receive a generic or job-description-based generation request.
2. Optimize the resume data through the configured AI integration.
3. Download the LaTeX template from Supabase storage.
4. Render the template through RazorLight.
5. Push the generated `.tex` file into the GitHub repository path `docs/<generated-name>.tex`.
6. Insert a job record into Supabase.
7. Trigger the GitHub Actions workflow that compiles the LaTeX file into PDF.
8. Send a workflow-started Telegram notification.

Supported generation endpoints:

- `POST /api/resume/generate/generic`
- `POST /api/resume/generate/jd`

Job status can be polled through:

- `GET /api/resume/status/{jobId}`

The most recent generated PDF URL is exposed through:

- `GET /api/resume/latest`

### Notification flow

`job-notifier` authenticates with:

- `POST /api/auth/login`

It then sends notification requests to:

- `POST /api/notification/send`

`NotificationService` maps the request body to one of three Telegram outcomes:

- in-progress notification when both `pdfUrl` and `errorMessage` are null
- failure notification when `errorMessage` is present
- success notification when `pdfUrl` is present

## API Surface

### Public endpoints

- `GET /api/health`
- `GET /api/about`
- `GET /api/project`
- `GET /api/experience`
- `GET /api/resume/latest`

### Auth endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh/{token}`
- `POST /api/auth/logout`

### Authenticated content and orchestration endpoints

- `POST /api/about`
- `PATCH /api/about/update/{id}`
- `POST /api/project`
- `GET /api/project/deleted`
- `GET /api/project/{id}`
- `PATCH /api/project/update/{id}`
- `PATCH /api/project/delete/{id}`
- `POST /api/experience`
- `GET /api/experience/deleted`
- `GET /api/experience/{id}`
- `PATCH /api/experience/update/{id}`
- `PATCH /api/experience/delete/{id}`
- `POST /api/contact`
- `GET /api/contact`
- `PATCH /api/contact/update/{id}`
- `POST /api/education`
- `GET /api/education`
- `PATCH /api/education/update/{id}`
- `POST /api/techstack`
- `GET /api/techstack`
- `PATCH /api/techstack/update/{id}`
- `POST /api/resume`
- `GET /api/resume`
- `PATCH /api/resume/update/{id}`
- `POST /api/resume/generate/generic`
- `POST /api/resume/generate/jd`
- `GET /api/resume/status/{jobId}`
- `POST /api/notification/send`

## Authentication And Middleware

- JWT bearer auth is configured in `Program.cs`.
- Most controllers are protected with `[Authorize]`.
- Public routes are explicitly marked with `[AllowAnonymous]`.
- A custom exception middleware translates application exceptions into HTTP responses.
- CORS is enabled with methods sourced from configuration and `AllowAnyOrigin`.

## Configuration

Start from:

```bash
cp src/WebAPI/appsettings.Sample.json src/WebAPI/appsettings.json
```

The sample config defines these settings sections:

- `CorsSettings`
  - `Hosts`
  - `Methods`
- `MongoDbSettings`
  - `Url`
  - `Db`
- `JwtSettings`
  - `AccessTokenSecretKey`
  - `RefreshTokenSecretKey`
  - `AccessTokenExpiryInMinutes`
  - `RefreshTokenExpiryInDays`
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

At startup the app also attempts to load:

```text
/etc/secrets/appsettings.Production.json
```

This is optional and is intended for mounted production secrets.

## Local Development

Prerequisites:

- .NET 9 SDK
- MongoDB
- valid configuration values in `src/WebAPI/appsettings.json`
- external credentials for the integrations you want to exercise

Run locally:

```bash
dotnet restore PortfolioApi.sln
dotnet run --project src/WebAPI/WebAPI.csproj --launch-profile dev
```

The `dev` launch profile binds to:

```text
http://localhost:5000
```

OpenAPI is only exposed in development:

```text
http://localhost:5000/openapi/v1.json
```

## Testing

Run the unit test project with:

```bash
dotnet test PortfolioApi.sln
```

The repository includes unit tests for services such as:

- about
- project
- experience
- education
- tech stack
- contact
- resume
- user/auth
- notifications

## Docker

Build:

```bash
docker build -t portfolio-api .
```

Run:

```bash
docker run --rm -p 8080:8080 portfolio-api
```

Container notes:

- the image uses the .NET 9 runtime
- the app listens on port `8080`
- `ASPNETCORE_ENVIRONMENT` is set to `Production`
- mounted secrets can be read from `/etc/secrets/appsettings.Production.json`

## Deployment

The repo includes GitHub Actions workflows for CI and deployment:

- `.github/workflows/build.yml`
- `.github/workflows/deploy-render.yml`

Current behavior described in the existing repo:

- CI restores, builds, and tests on pushes to `develop` and PRs targeting `master`
- deployment validates and deploys to Render on pushes to `master`

## Integration Notes

- `portfolio-website` expects this API to expose public routes for about, project, experience, and latest resume lookup.
- `job-notifier` authenticates here and sends notification events here.
- The API acts as the coordination boundary between MongoDB-managed content and Supabase/GitHub-managed resume artifacts.

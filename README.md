# Portfolio API

Technical deep dive: [docs/TECHNICAL_DOC.md](docs/TECHNICAL_DOC.md)

I built `portfolio-api` to act as the single source of truth for my portfolio and resume workflow. Instead of keeping content, resume data, job-specific tailoring, storage state, and delivery logic spread across separate tools, this API keeps the system coordinated from one place.

## Problem Statement

The hard part of a portfolio system is not just serving profile data. It is keeping content, resume generation, storage, notifications, and frontend delivery in sync without duplicating state across multiple services and manual steps.

I wanted one backend that could:

- manage the public portfolio content shown on my website
- generate generic and job-tailored resumes from the same source data
- track resume build jobs across GitHub Actions and Supabase
- expose the final resume artifact back to the website and notification pipeline

## Why It Matters

- It reduces context switching between content management, resume generation, and delivery.
- It gives the rest of the system one place to ask for portfolio data or resume state.
- It turns a scattered workflow into a repeatable pipeline I can extend over time.

## Role In The System

`portfolio-api` sits in the middle of the four-project setup:

1. `portfolio-website` reads public portfolio content and the latest resume URL from this API.
2. Resume generation requests are created here and pushed into the LaTeX compilation pipeline.
3. `latex-compiler` and Supabase update job status as the resume moves through generation.
4. `job-notifier` authenticates against this API and triggers outbound notifications as job status changes.

## How It Works

At a high level, the API does three jobs:

- serves portfolio resources such as about, projects, experience, education, contact, tech stack, and resume data
- secures admin and orchestration flows with JWT-based authentication
- coordinates integrations with MongoDB, Supabase, GitHub, AI services, and Telegram

## Quickstart

```bash
cp src/WebAPI/appsettings.Sample.json src/WebAPI/appsettings.json
dotnet restore PortfolioApi.sln
dotnet run --project src/WebAPI/WebAPI.csproj --launch-profile dev
```

The development profile starts the API on `http://localhost:5000`.

If you want the implementation details, configuration shape, API surface, and deployment notes, use [docs/TECHNICAL_DOC.md](docs/TECHNICAL_DOC.md).

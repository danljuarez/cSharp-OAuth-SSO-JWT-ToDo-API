# Project Walkthrough: OAuth SSO JWT ToDo API
[![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet)](https://dotnet.microsoft.com/en-us/)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/status-in--progress-yellow.svg)](#)

#### Author: Daniel Juarez

This walkthrough offers a technical overview of the `OauthSSOJwtTodoApiBackend` project, intended solely for demonstration purposes. It focuses on authentication strategies, architectural decisions, extensibility, and code quality practices that align with enterprise software development.

## Overview

A full-featured REST Web API built with C# 13 and ASP.NET Core 9.0, presenting modern authentication (OAuth2 PKCE via LinkedIn), JWT-based authorization, EF Core integration, Swagger UI, and clean architecture patterns, with the goal of demonstrating secure backend development best practices.

## Key Features

* OAuth 2.0 Single Sign-On with PKCE via LinkedIn (supports real & mock for development)
* Login / Logout with JWT-based authentication with refresh token rotation & invalidation
* Optional secure cookie-based authentication (HttpOnly, Secure, SameSite=Strict)
* Rate limiting per IP (fully configurable)
* Secure refresh token handling
* Configurable CORS policy per environment
* Role-based authorization (User, Manager, Admin)
* Enum-based service responses (TodoOperationResult)
* Future extensibility for multi-DB provider support
* In-memory database currently used for development, testing and presentation purposes only
* Modular architecture with extensibility in mind
* Clean, testable architecture aligned with SOLID principles
* Structured configuration via `appsettings.*.json`
* Organized `Program.cs` via modular startup helpers
* Centralized exception handling middleware
* Logger service (extensible for Seq, Sentry, etc.)
* Fully documented API with **Swagger/OpenAPI** (including JWT & OAuth2 flows)

## Project Status: In Progress
* **Current Phase**: Core architecture is implemented and stable.
* **Environment**: Currently in `development mode`.
* **Database**: `In-Memory` only (future support planned for: PostgreSQL, SQL Server, SQLite, MySQL, Oracle).

## Authentication & Authorization

* **OAuth2.0 with PKCE**<br>
  Supports Authorization Code Flow using LinkedIn’s OAuth2 endpoints. Secure handling of code exchange and token retrieval.

* **JWT Access Tokens**<br>
  Tokens are signed using a symmetric key and validated using `Microsoft.IdentityModel.Tokens`. Claims are extracted and mapped consistently to the `ClaimsPrincipal`.

* **Refresh Tokens**<br>
  Refresh tokens are stored per user and managed securely.

* **Dual Authentication Support**

  * JWT via `Authorization: Bearer <token>`
  * Secure cookies (`HttpOnly`, `SameSite=Strict`, `SecurePolicy=Always`)

* **Role-based Access Control (RBAC)**<br>
  `User`, `Manager`, and `Admin` roles are enforced via `[Authorize(Roles = "...")]`.

## Architecture

### Core Layers

* **Controllers**: Define API endpoints and enforce role-based access using authorization attributes.
* **Services**: Implement business logic, enforce data access boundaries, and return tuple-based service responses.
* **Data Access**: EF Core-backed `DbContext` with per-entity configuration and relationship mappings.

### Helpers

* `JwtHelper`, `ClaimsPrincipalExtensions`, `CorsHelper`, `RateLimitingHelper`, `SwaggerAuthHelper`, etc. abstract out concerns from `Program.cs` improving modularity, reusability, and testability.

### Middleware

* `JwtCookieAuthMiddleware` supports cookie-based JWT parsing for browser clients.
* `ExceptionMiddleware` ensures structured error responses.

## Testing & Seeding Data

* In-memory database mode is enabled by default for development and presentation purposes.
* Sample seed data (users and todos) is loaded from `resources/SeedData.json`.
* Sensitive information in the sample seed data (e.g., emails and passwords) has been intentionally left in plain text for testing and demonstration purposes only.
* Seeding is managed by the `SeedService`, which delegates responsibility to `IUserSeeder` and `ITodoSeeder` for user and to-do data respectively.

## API Documentation

* OpenAPI docs configured with:

  * JWT Bearer scheme
  * `oauth2` scheme with `AuthorizationCode` flow
* Redirect URI, clientId/secret dynamically set from config


## Tech Stack

| Layer        | Technology                      |
|--------------|----------------------------------|
| Backend      | C# 13 ASP.NET Core 9 Web API     |
| Auth         | LinkedIn OAuth2 + PKCE + JWT     |
| ORM          | EF Core                          |
| Docs         | Swagger / OpenAPI                |
| Middleware   | Custom: JWT from Cookie, Errors  |
| Logging      | `ILogger<T>` (extensible)        |
| Rate Limiting| .NET 9 Rate Limiter Middleware   |

## Development Setup

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Visual Studio](https://visualstudio.microsoft.com/) 2022 v17.14+
* Docker Desktop *(optional – container support coming soon)*

## Try It Locally (Development Mode)

```bash
# Install dependencies
dotnet restore

# Builds the project
dotnet build

# Run API (dev mode)
dotnet run
```

Then navigate to:<br>
[http://localhost:XXXX/swagger]()

> First, log in using the `/auth` endpoint in Swagger with a valid user, then use the Bearer authentication feature to authorize and access protected endpoints. See `resources/SeedData.json` for sample users.

## What’s Next?

* Complete the addition of XML documentation to classes.
* Health check endpoint.
* Unit tests coverage with `xUnit` + Moq
* Integration tests coverage
* Dockerfile
* Frontend SPA integration (React/Angular)


## Evaluation Notes

This project is actively being developed and reflects enterprise-grade architectural practices. It is currently operating in development mode with a focus on foundational security, extensibility, and maintainability.

Feedback is welcome, including constructive comments, suggestions, and ideas for improvement.

<br/>
Thank you.

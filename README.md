# 3R-Model (NiRAProject)

This repository contains the **3R-Model** backend project for the **Nigerian Internet Registration Association (NiRA)**. It implements the Registrar–Registrar–Registry (3R) model infrastructure used to manage `.ng` domain registrations.

---

##  Table of Contents

- [Overview](#overview)  
- [Technologies](#technologies)  
- [Repository Structure](#repository-structure)  
- [Configuration](#configuration)  
- [Setup & Run](#setup--run)  
- [Database & Migrations](#database--migrations)  
- [Folder Details](#folder-details)  
- [License](#license)

---

##  Overview

The **3R-Model** project implements the registry architecture for domain management in Nigeria under the `.ng` domain. It handles registrar applications, approvals, and other workflows using ASP.NET Core, typically backed by a database.

---

##  Technologies

- **.NET 8.0** (C#)  
- **ASP.NET Core Web API**  
- **Entity Framework Core** for data persistence and migrations  
- Dependency injection architecture across `controllers`, `Tools`, etc.

---

##  Repository Structure

- **`Program.cs`** – Entry point and web host configuration.  
- **`NiRAProject.csproj`** – Project file with dependencies and metadata.  
- **`appsettings.json` & `appsettings.Development.json`** – Application configuration (e.g., database, email, other secrets).  
- **`controllers/`** – API controllers managing HTTP routes and endpoint logic.  
- **`models/`** – Domain models / entity definitions.  
- **`Dtos/`** – Data Transfer Objects for input/output shaping.  
- **`Data/`** – Database context and data access logic.  
- **`Migrations/`** – EF Core database migration files.  
- **`Tools/`** – Utility services (e.g., email sending, helper classes).
- **`NiRAProject.http`** – Optional HTTP client test definitions (used in tools like VS Code REST Client).

---

##  Configuration

Before running the app, configure your settings in `appsettings.json` or your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-connection-string-here>"
  }
}

1:https://chatgpt.com/share/6a7505bf-56e4-83ea-a6ab-063ef2be1e98

https://chatgpt.com/share/6a5bc383-249c-83ea-9c27-c21d788925ae

2: Explain Backend With details   https://chatgpt.com/share/6a592e7f-09a4-83ea-8926-84f2587fbb98

3: Frontend        https://chatgpt.com/share/6a668cc5-8db0-83ea-9ab8-39e480dcad56      |   https://claude.ai/share/e0b9bf62-de9c-4918-abba-3db8cd4a8856     For Claud  


this Script Write in Claud after Send Json Data To This Chat
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
I have now uploaded the final Salamtak OpenAPI JSON, UI reference images, journey videos, and available assets.

Start with Milestone 0 only.

Audit the complete OpenAPI file, not a truncated preview. Inspect all designs and journey videos carefully.

Do not generate Angular code yet.

Produce the required audit, traceability matrix, backend gap report, route map, proposed architecture, asset gap list, blocking questions, and implementation milestones.

The frontend must match the supplied Salamtak design and must not contain any functional action that is unsupported by the backend.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



4:Presnetation   https://canva.link/ks8837zr46934o8 

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
# 🩺 Salamtak — Full-Stack Medical Appointment & Healthcare Platform

Salamtak is a **full-stack healthcare and medical appointment platform** designed to connect **patients, doctors, and administrators** within one integrated system.

The platform manages the complete healthcare appointment lifecycle — from discovering doctors and clinics, checking available time slots, and booking appointments, to doctor verification, medical reports, prescriptions, feedback, document management, and real-time notifications.

The backend is built using **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**, while the system also supports real-time communication using **SignalR** and integrations for AI, email, payment processing, and private file storage.

---

# 📌 Project Overview

Healthcare booking systems involve more than simply creating appointments.

A real medical platform must handle:

* Different types of users and permissions.
* Verified doctors.
* Medical specialties.
* Clinics.
* Doctor-to-clinic relationships.
* Doctor schedules.
* Appointment availability.
* Booking conflicts.
* Appointment lifecycle management.
* Medical reports.
* Prescriptions.
* File attachments.
* Patient feedback.
* Doctor documents.
* Notifications.
* Security.
* Validation.
* API protection.
* Real-time communication.

Salamtak was designed to manage these concerns through a modular layered architecture.

---

# 👥 System Roles

The platform supports three main roles:

## 👤 Patient

Patients can interact with the medical system and manage their healthcare journey.

Main capabilities include:

* Register and log in.
* Search for doctors.
* Browse doctors by specialty.
* View doctor profiles.
* View clinics.
* View available appointment slots.
* Book appointments.
* Cancel appointments according to business rules.
* View appointment information.
* Receive notifications.
* Access medical reports.
* Access prescriptions and report attachments.
* Submit feedback after completed appointments.

---

## 👨‍⚕️ Doctor

Doctors manage their medical activities through the platform.

Main capabilities include:

* Register as a doctor.
* Upload verification documents.
* Wait for administrator verification.
* Manage doctor profile information.
* Work with multiple clinics.
* Manage availability slots.
* View appointments.
* Complete appointments.
* Create medical report entries.
* Add prescriptions.
* Attach files to medical reports.
* Receive appointment-related notifications.

A newly registered doctor starts with a pending verification state and must be verified before participating fully in medical workflows.

---

## 🛡️ Admin

Administrators supervise platform operations.

Important responsibilities include:

* Review doctor information.
* Review doctor verification documents.
* Verify or reject doctors.
* Record verification information.
* Control administrative workflows.
* Protect the platform from unverified healthcare providers.

---

# 🏗️ System Architecture

Salamtak follows a layered backend architecture.

```text
┌─────────────────────────────┐
│        Frontend Client      │
│          Angular UI         │
└──────────────┬──────────────┘
               │
               │ HTTP / JWT / SignalR
               ▼
┌─────────────────────────────┐
│        API / Controllers    │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│         Service Layer       │
│       Business Logic        │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│         Unit of Work        │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│     Generic Repositories    │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│      Entity Framework Core  │
│           DbContext         │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│          SQL Server         │
└─────────────────────────────┘
```

Cross-cutting concerns include:

```text
JWT Authentication
Role-Based Authorization
AutoMapper
FluentValidation
Exception Middleware
Output Caching
Rate Limiting
SignalR
File Storage
Email Integration
AI Integration
Payment Integration
```

---

# 🔄 Backend Request Flow

A typical API request follows this flow:

```text
Frontend
   ↓
Controller
   ↓
Service
   ↓
Unit of Work
   ↓
Repository
   ↓
EF Core
   ↓
SQL Server
```

Controllers remain relatively thin, while business rules are handled inside the service layer.

This improves:

* Separation of concerns.
* Maintainability.
* Testability.
* Reusability.
* Code organization.

---

# 🔐 Authentication & Authorization

Salamtak uses **JWT-based authentication**.

After a successful login, the backend generates a JWT containing claims such as:

```text
User Id
Email
Role
Name Identifier
```

These claims are used by protected endpoints to identify the currently authenticated user.

The system supports role-based authorization for:

```text
Admin
Doctor
Patient
```

For example, endpoints intended only for doctors or administrators can be protected using role-based authorization policies.

---

# 👨‍⚕️ Doctor Management

The Doctor module handles doctor-related operations.

It includes:

* Doctor registration.
* Doctor profiles.
* Doctor search.
* Doctor verification status.
* Doctor specialties.
* Doctor clinics.
* Doctor documents.
* Doctor availability.
* Doctor appointments.
* Doctor feedback.

Doctors are connected to specialties and may work in multiple clinics.

---

# 🏥 Specialties

The platform supports medical specialties.

Examples may include:

```text
Cardiology
Dermatology
Dentistry
Neurology
Orthopedics
```

A specialty may contain multiple doctors.

Conceptually:

```text
Specialty
   │
   ├── Doctor A
   ├── Doctor B
   └── Doctor C
```

This allows patients to discover doctors based on the type of medical care they need.

---

# 🏢 Clinics

Clinics represent physical or organizational locations where doctors provide medical services.

Doctors can be associated with multiple clinics.

The relationship is represented through a doctor-clinic relationship.

```text
Doctor
   │
   ▼
DoctorClinic
   │
   ▼
Clinic
```

This creates a many-to-many relationship:

```text
One Doctor → Multiple Clinics

One Clinic → Multiple Doctors
```

The relationship is also important when validating appointment slots because an availability slot must belong to the correct doctor and clinic.

---

# 📅 Availability Slot Management

Doctors can manage appointment availability through availability slots.

A slot represents a specific period during which a doctor can receive an appointment.

Example:

```text
Doctor: Dr. Ahmed
Clinic: Cairo Clinic

10:00 AM → Available
10:30 AM → Available
11:00 AM → Booked
11:30 AM → Available
```

The system performs business-rule checks including:

* Doctor validation.
* Clinic validation.
* Doctor-clinic relationship validation.
* Slot ownership validation.
* Future-date validation.
* Availability validation.
* Slot overlap validation.

This prevents invalid schedules from being created.

---

# 📆 Appointment Management

Appointment management is one of the core modules of Salamtak.

The system supports the appointment lifecycle including:

```text
Booking
Cancellation
Completion
```

---

# ✅ Appointment Booking Flow

When a patient books an appointment, the backend performs several validation steps.

## Step 1 — Identify the Patient

The patient identity is retrieved from the authenticated JWT.

```text
JWT
 ↓
Current User Id
 ↓
Patient
```

---

## Step 2 — Validate the Patient

The service verifies that the patient exists.

---

## Step 3 — Validate the Doctor

The service verifies that:

* The doctor exists.
* The doctor is verified.

---

## Step 4 — Validate the Clinic

The backend verifies that:

* The clinic exists.
* The doctor is associated with the selected clinic.

---

## Step 5 — Validate the Availability Slot

The slot must:

* Exist.
* Belong to the selected doctor.
* Belong to the selected clinic.
* Be available.
* Be scheduled in the future.

---

## Step 6 — Prevent Invalid Booking

The system checks whether the selected slot already has an appointment.

---

## Step 7 — Reserve the Slot

Once validation succeeds:

```text
Slot.IsAvailable = false
```

---

## Step 8 — Create Appointment

A new appointment is created with its initial confirmed state.

Conceptually:

```text
Patient
   │
   ▼
Select Doctor
   │
   ▼
Select Clinic
   │
   ▼
Select Available Slot
   │
   ▼
Validate Booking
   │
   ▼
Create Appointment
   │
   ▼
Reserve Slot
```

---

## Step 9 — Save Changes

Changes are persisted through:

```text
Service
 ↓
UnitOfWork
 ↓
SaveChangesAsync
 ↓
SQL Server
```

---

## Step 10 — Create Notification

After the appointment operation, a persistent notification is created.

---

## Step 11 — Real-Time Notification

The application then attempts to notify the relevant user using SignalR.

```text
Appointment Created
        ↓
Notification Saved
        ↓
SignalR Push
```

This persistence-first approach makes notifications more reliable because the notification remains stored even if the client is temporarily disconnected from SignalR.

---

# ❌ Appointment Cancellation

Appointments can also be cancelled according to business rules.

The cancellation flow validates:

* The appointment exists.
* The caller owns or is authorized to manage the appointment.
* The appointment has not already been completed.
* The cancellation does not violate the configured time restriction.

The current rules include preventing cancellation within 24 hours of the appointment start time.

When cancellation succeeds:

```text
Appointment
Confirmed
   ↓
Cancelled
```

The related slot becomes available again:

```text
Slot
Unavailable
   ↓
Available
```

The system then persists the changes and creates the required notifications.

---

# ✔️ Appointment Completion

Doctors can mark appointments as completed.

The backend verifies that:

* The appointment exists.
* The requesting doctor owns the appointment.
* The operation is valid.

After completion:

```text
Appointment.Status = Completed
```

The patient may then become eligible for other features such as:

* Medical reports.
* Prescriptions.
* Feedback.

---

# 📝 Medical Reports

The medical-report module allows doctors to document medical information after completed appointments.

The system supports:

* Medical report entries.
* Attachments.
* Prescriptions.

Before a medical report entry is created, the backend checks:

* The appointment exists.
* The appointment belongs to the doctor.
* The appointment is completed.

Conceptually:

```text
Completed Appointment
        ↓
Doctor
        ↓
Medical Report
        ↓
Report Entries
        ↓
Attachments / Prescriptions
```

This protects medical data from being created by unrelated doctors.

---

# 📎 Medical Attachments

Medical reports can include attachments through the file-storage abstraction.

The project uses storage interfaces such as:

```text
IFileStorageService
IPrivateFileStorageService
```

The purpose of this abstraction is to separate application logic from the physical storage implementation.

This makes it possible to change storage providers later without rewriting medical-report business logic.

Potential implementations may include:

```text
Local Private Storage
Cloud Object Storage
Azure Blob Storage
AWS S3
```

The current project still requires the storage abstractions and DI registrations to be unified consistently.

---

# 📄 Doctor Document Verification

Doctors can upload verification documents.

The doctor verification lifecycle is conceptually:

```text
Doctor Registration
       ↓
Verification Pending
       ↓
Upload Documents
       ↓
Admin Review
       ↓
Approve / Reject
```

When an administrator verifies a doctor, verification-related information is stored, such as:

```text
IsVerified
VerifiedByAdminId
VerifiedAt
```

This protects the platform from allowing unverified users to operate as medical professionals.

---

# ⭐ Feedback System

Patients can submit feedback about doctors.

Feedback is protected by business rules.

A patient may submit feedback only when:

* The related appointment exists.
* The appointment has been completed.
* The patient is eligible to review that appointment.
* Duplicate feedback for the same appointment does not already exist.

The module supports:

```text
Create Feedback
Update Feedback
Delete Feedback
Read Feedback
```

---

# ⚡ Output Caching

Doctor feedback read operations use ASP.NET Core Output Caching.

A cache policy named:

```text
PublicShort
```

is configured with a short expiration period.

The feedback cache is tagged using:

```text
doctor-feedbacks
```

When feedback is modified, the application invalidates the cached responses.

Example:

```text
GET Doctor Feedback
       ↓
Cache Result

Update Feedback
       ↓
Evict Cache

Next GET
       ↓
Fresh Data
```

This approach improves read performance while reducing the risk of returning stale feedback after write operations.

---

# 🚦 API Rate Limiting

Salamtak uses ASP.NET Core Rate Limiting to protect endpoints from excessive requests.

Configured policies include examples such as:

```text
PatientRead
60 requests / minute / user

PatientWrite
10 requests / minute / user

FeedbackWrite
5 requests / minute / user

FeedbackRead
30 requests / minute / IP
```

When the rate limit is exceeded, the API returns:

```http
HTTP 429 Too Many Requests
```

with a structured JSON error response.

Rate limiting helps protect against:

* API spam.
* Accidental request loops.
* Excessive write operations.
* Basic abuse scenarios.

Some policies are configured but still need to be applied more consistently across the API.

---

# 🔔 Notification System

Salamtak supports both:

## Persistent Notifications

Notifications are stored in SQL Server.

This allows users to retrieve notifications even after disconnecting from the application.

---

## Real-Time Notifications

SignalR is used to push notifications instantly to connected users.

The notification flow is:

```text
Business Operation
        ↓
Create Notification Entity
        ↓
Save Notification to Database
        ↓
SignalR
        ↓
Connected Frontend Client
```

The backend uses an abstraction similar to:

```text
IRealtimeNotificationService
```

with a SignalR-based implementation.

This keeps business services decoupled from the actual SignalR infrastructure.

---

# 📡 SignalR

SignalR is configured through the ASP.NET Core backend and a notification hub.

Conceptually:

```text
ASP.NET Core
      ↓
NotificationHub
      ↓
SignalR Connection
      ↓
Frontend
```

The frontend should establish the authenticated SignalR connection using the JWT access token.

For reliable notifications, the frontend should also retrieve persisted notifications after reconnecting so that notifications missed during connection loss are not lost.

---

# 🤖 AI Symptom Analysis

Salamtak includes an AI integration abstraction:

```text
IAiChatService
```

with an implementation based on:

```text
Gemini / Google GenAI
```

The AI module provides a symptom-analysis endpoint.

This allows the healthcare platform to integrate AI-assisted functionality while keeping the AI provider behind an application abstraction.

Conceptually:

```text
Frontend
   ↓
AI Endpoint
   ↓
IAiChatService
   ↓
GeminiChatService
   ↓
Google GenAI
```

The AI feature should be considered an assistance feature rather than a replacement for professional medical diagnosis.

---

# 💳 Payment Integration

The project includes payment integration scaffolding based on Paymob.

The payment architecture includes components such as:

```text
IPaymobClient
PaymobOptions
HttpClient
PaymentService
```

This separates the payment provider from the rest of the application.

At the current project stage, payment support should be treated as integration scaffolding / an evolving module rather than describing it as a fully production-complete payment workflow unless the complete Paymob callback and verification flow is implemented.

---

# 📧 Email Integration

The project contains an external email service using SMTP.

Email functionality is separated from the main application logic through an external service layer.

Conceptually:

```text
Application Service
       ↓
Email Abstraction
       ↓
SMTP Email Service
       ↓
Mail Server
```

This architecture keeps email delivery concerns isolated from business services.

---

# 🗃️ Data Access Architecture

Salamtak uses:

```text
Entity Framework Core
SQL Server
Generic Repository
Unit of Work
```

---

# 📦 Repository Pattern

The project contains:

```text
IGenericRepository<T>
```

with common data-access operations such as:

```text
Add
Update
Delete
GetAll
GetById
FirstOrDefault
Any
```

Instead of putting EF Core queries throughout controllers, repositories provide a common data-access abstraction.

Benefits include:

* Reusable data access.
* Reduced duplication.
* Better separation between business and persistence logic.
* Easier testing and maintenance.

---

# 🔄 Unit of Work Pattern

The Unit of Work coordinates repositories and database commits.

It exposes database persistence through:

```text
SaveChangesAsync(CancellationToken)
```

Conceptually:

```text
AppointmentService
       ↓
Appointment Repository
       ↓
Slot Repository
       ↓
Notification Repository
       ↓
UnitOfWork.SaveChangesAsync()
```

This creates a single coordination point for persistence.

---

# 🧠 Service Layer Pattern

Business logic is placed inside application services rather than controllers.

For example:

```text
AppointmentController
        ↓
AppointmentService
```

The service is responsible for operations such as:

* Validation.
* Doctor checks.
* Patient checks.
* Clinic checks.
* Slot checks.
* Appointment business rules.
* Persistence coordination.
* Notification creation.

This keeps controllers focused primarily on HTTP concerns.

---

# 💉 Dependency Injection

Dependency Injection is used throughout the application.

Typical dependencies include abstractions for:

```text
Repositories
Unit of Work
Services
AutoMapper
AI Services
Email Services
Payment Services
File Storage
Realtime Notifications
```

Benefits include:

* Loose coupling.
* Easier testing.
* Replaceable implementations.
* Cleaner architecture.

---

# 📤 DTO Pattern

The application does not rely on directly exposing database entities to clients.

DTOs are used as API contracts.

Conceptually:

```text
Database Entity
      ↓
AutoMapper
      ↓
DTO
      ↓
API Response
```

DTOs provide:

* API contract control.
* Better security.
* Reduced coupling between API and database entities.
* Cleaner frontend responses.

---

# 🗺️ AutoMapper

AutoMapper is used to convert between entities and DTOs.

Mapping configuration is centralized through mapping profiles.

Instead of manually repeating property assignments in every service:

```text
Entity
   ↓
MappingProfile
   ↓
DTO
```

This reduces mapping boilerplate and keeps mapping logic centralized.

---

# ✅ FluentValidation

Input validation is implemented using FluentValidation.

Validators are registered through dependency injection and validate request DTOs.

Typical flow:

```text
Request DTO
    ↓
FluentValidator
    ↓
Valid?
 ┌──┴──┐
Yes    No
 │      │
Service AppValidationException
```

This separates input validation from business logic.

---

# 🚨 Centralized Exception Handling

The project contains a custom exception hierarchy based on:

```text
BaseAppException
```

Specialized exceptions include:

```text
AppValidationException → HTTP 400
BadRequestException    → HTTP 400
ForbiddenException     → HTTP 403
NotFoundException      → HTTP 404
ConflictException      → HTTP 409
```

A centralized Exception Middleware converts exceptions into consistent JSON API responses.

Conceptually:

```text
Service
   ↓
Exception
   ↓
ExceptionMiddleware
   ↓
HTTP Status Code
   ↓
Structured JSON Error
```

This avoids repetitive try/catch blocks inside every controller.

---

# 🗑️ Soft Delete

Some entities support soft deletion through:

```text
IsDeleted
```

Instead of physically deleting the database record:

```text
DELETE FROM Entity
```

the entity can be marked as:

```text
IsDeleted = true
```

This helps preserve historical information.

A future improvement is to enforce global EF Core query filters consistently for all soft-deletable entities.

---

# ⛔ CancellationToken Support

The persistence layer already supports:

```csharp
SaveChangesAsync(CancellationToken)
```

Some controllers and services propagate request cancellation correctly.

The intended flow is:

```text
Client Cancels Request
        ↓
HttpContext.RequestAborted
        ↓
Controller CancellationToken
        ↓
Service
        ↓
Repository / UnitOfWork
        ↓
EF Core
```

This prevents unnecessary work after a client disconnects.

CancellationToken propagation is currently being standardized across all critical endpoints.

---

# 🌱 JSON Data Seeding

The project supports startup data seeding using JSON files located inside the persistence project.

Conceptually:

```text
Application Startup
       ↓
Read JSON Files
       ↓
Check Existing Records
       ↓
Insert Missing Data
       ↓
SaveChanges
```

The JSON files must be copied to the build output directory.

Example project configuration:

```xml
<ItemGroup>
  <Content Include="Json Files\**\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

This ensures the seeding files remain available after build/publish.

---

# 🧩 Design Patterns & Architectural Practices

The backend applies several patterns and architectural practices.

## Repository Pattern

Provides reusable abstraction over data access.

```text
Service
  ↓
Repository
  ↓
EF Core
```

---

## Unit of Work Pattern

Coordinates repositories and database persistence.

```text
Multiple Repository Changes
          ↓
UnitOfWork
          ↓
SaveChanges
```

---

## Service Layer Pattern

Keeps business logic outside controllers.

```text
Controller
   ↓
Service
   ↓
Persistence
```

---

## Dependency Injection

Application components depend on abstractions rather than creating implementations directly.

---

## DTO Pattern

Separates API contracts from persistence entities.

---

## Mapper Pattern

AutoMapper centralizes entity-to-DTO transformations.

---

## Middleware Pattern

Used for centralized exception handling and HTTP request pipeline concerns.

---

## Provider / Strategy-Style Abstractions

External dependencies such as file storage are accessed through interfaces, allowing their implementations to be replaced independently.

---

## Soft Delete Pattern

Preserves records while preventing deleted data from appearing in normal operations.

---

# ❗ Specification Pattern

The current implementation does **not** use the Specification Pattern.

Repository queries currently rely primarily on standard expressions/lambda queries.

Specification could be introduced later if query filtering and composition become significantly more complex.

---

# ⚔️ Concurrency Considerations

Appointment booking is a concurrency-sensitive operation.

A service-level availability check alone cannot guarantee that two simultaneous requests do not attempt to reserve the same slot.

Example:

```text
Request A → Slot Available
Request B → Slot Available

Request A → Book
Request B → Book
```

To fully protect the system, the database should enforce uniqueness on the appointment slot.

Recommended EF Core configuration:

```csharp
builder
    .HasIndex(a => a.AvailabilitySlotId)
    .IsUnique();
```

The booking workflow should also use proper transactional handling and catch database concurrency conflicts.

---

# 🔒 Security Hardening

The current project review identified several security improvements before production deployment.

## Password Hashing

The current authentication implementation uses SHA-256 password hashing.

SHA-256 is not suitable as a password hashing algorithm because it is intentionally fast.

It should be replaced with a password-specific hashing implementation such as:

```text
ASP.NET Core IPasswordHasher<User>
```

or a secure algorithm such as BCrypt or Argon2.

Example registration:

```csharp
services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

---

# 🔁 Refresh Tokens

The current JWT implementation issues access tokens, but refresh-token support is not currently implemented.

Adding refresh tokens would improve the frontend authentication experience by allowing users to obtain new access tokens without logging in again.

This is currently considered an optional enhancement.

---

# 🧵 Transaction Management

Multi-step workflows such as appointment booking and cancellation benefit from explicit transaction handling.

For example:

```text
Start Transaction
      ↓
Validate Slot
      ↓
Reserve Slot
      ↓
Create Appointment
      ↓
Save
      ↓
Commit
```

If any critical step fails:

```text
Rollback
```

This prevents partially completed medical booking operations.

---

# 🧾 Optimistic Concurrency

Entities such as:

```text
Appointment
AvailabilitySlot
```

may benefit from optimistic concurrency tokens such as:

```text
RowVersion
```

This prevents one request from unknowingly overwriting changes made by another concurrent request.

---

# 🗄️ Distributed Infrastructure

The current caching configuration is suitable for a single application instance.

For multi-server production deployment, the recommended architecture includes:

```text
Redis Distributed Cache
Distributed Rate Limiting
```

This allows multiple backend instances to share infrastructure state.

---

# 🧪 Testing Roadmap

High-value integration tests should focus on critical workflows such as:

## Appointment Booking

Test:

```text
Valid booking
Unavailable slot
Unverified doctor
Invalid clinic
Double booking
Past slot
```

## Appointment Cancellation

Test:

```text
Authorized cancellation
Unauthorized cancellation
Completed appointment
24-hour cancellation restriction
```

## Notifications

Test:

```text
Notification persisted
Realtime delivery succeeds
Realtime delivery fails
User reconnects
```

## Feedback

Test:

```text
Completed appointment
Incomplete appointment
Duplicate feedback
Cache invalidation
```

---

# 📊 Core System Flow

The complete patient journey can be summarized as:

```text
Register / Login
      ↓
Browse Specialties
      ↓
Search Doctors
      ↓
View Doctor Profile
      ↓
Select Clinic
      ↓
View Available Slots
      ↓
Book Appointment
      ↓
Receive Notification
      ↓
Attend Appointment
      ↓
Doctor Completes Appointment
      ↓
Medical Report / Prescription
      ↓
Patient Views Medical Information
      ↓
Patient Submits Feedback
```

---

# 👨‍⚕️ Doctor Journey

```text
Register Doctor
      ↓
Upload Documents
      ↓
Pending Verification
      ↓
Admin Review
      ↓
Doctor Verified
      ↓
Manage Clinics
      ↓
Manage Availability
      ↓
Receive Appointments
      ↓
Complete Appointment
      ↓
Create Medical Report
      ↓
Add Prescription / Attachments
```

---

# 🛡️ Admin Journey

```text
Admin Login
     ↓
Review Doctor
     ↓
Review Documents
     ↓
Approve / Reject
     ↓
Store Verification Information
     ↓
Manage Platform Operations
```

---

# 🔔 Notification Architecture

```text
Appointment / Business Event
            ↓
    Notification Service
            ↓
┌───────────┴───────────┐
│                       │
▼                       ▼
SQL Server            SignalR
Persistent             Realtime
Notification             Push
│                       │
└───────────┬───────────┘
            ▼
          User
```

This design combines durability with real-time user experience.

---

# 🛠️ Technology Stack

## Frontend

```text
Angular
HTML
CSS
TypeScript
```

## Backend

```text
C#
ASP.NET Core Web API
Entity Framework Core
SQL Server
```

## Authentication & Security

```text
JWT
Role-Based Authorization
ASP.NET Core Rate Limiting
```

## Validation & Mapping

```text
FluentValidation
AutoMapper
DTOs
```

## Realtime Communication

```text
SignalR
```

## Performance

```text
Output Caching
Cache Invalidation
```

## External Integrations

```text
Google Gemini / GenAI
Paymob
SMTP Email
Private File Storage
```

## Architecture

```text
Layered Architecture
Repository Pattern
Unit of Work
Service Layer
Dependency Injection
Middleware
```

---

# 📂 Conceptual Project Structure

```text
Salamtak
│
├── API
│   ├── Controllers
│   ├── Middleware
│   └── Program.cs
│
├── Application / Services
│   ├── Interfaces
│   ├── Services
│   ├── Validators
│   ├── DTOs
│   └── Mapping
│
├── Domain
│   ├── Entities
│   ├── Enums
│   └── Contracts
│
├── Persistence
│   ├── DbContext
│   ├── Repositories
│   ├── UnitOfWork
│   ├── Configurations
│   ├── Migrations
│   └── Json Files
│
├── External Services
│   ├── Email
│   ├── AI
│   ├── Payments
│   └── File Storage
│
└── Frontend
    └── Angular Application
```

> The exact physical folder names may differ slightly from this conceptual view; this section represents the architectural responsibilities of the solution.

---

# 🚀 Development Status

The application already covers the major functional requirements expected from a medical appointment platform:

* Authentication.
* Role-based authorization.
* Patients.
* Doctors.
* Doctor verification.
* Specialties.
* Clinics.
* Doctor-clinic relationships.
* Availability slots.
* Appointment lifecycle.
* Medical reports.
* Prescriptions.
* Attachments.
* Feedback.
* Doctor documents.
* Notifications.
* SignalR.
* Output caching.
* Rate limiting.
* Validation.
* Exception handling.
* AI integration.
* Payment integration scaffolding.
* Email services.
* Data seeding.

---

# 🛣️ Current Improvement Roadmap

Before considering the backend production-ready, the highest-priority improvements are:

1. Replace SHA-256 password hashing with a password-specific secure hasher.
2. Unify private file-storage dependency injection.
3. Protect appointment booking against race conditions with database uniqueness and transactions.
4. Propagate `CancellationToken` consistently.
5. Add explicit transactions around critical multi-step workflows.
6. Make SignalR delivery best-effort so realtime failures cannot break successful business operations.
7. Add consistent global soft-delete query filters.
8. Add optimistic concurrency where needed.
9. Ensure JSON seed files are copied correctly during build and deployment.
10. Add refresh-token support if required.
11. Add integration tests for booking and notification workflows.
12. Move caching and rate-limiting state to distributed infrastructure for multi-node deployments.

---

# 🎯 Project Goal

The main goal of Salamtak is to provide a structured, secure, scalable healthcare platform where:

* Patients can easily discover and book doctors.
* Doctors can manage their schedules and medical work.
* Administrators can verify healthcare professionals.
* Medical information can be managed safely.
* Users receive reliable notifications.
* Business rules remain centralized inside the backend.
* External services remain loosely coupled from core business logic.

The project also demonstrates practical use of modern backend engineering concepts including:

```text
REST API Design
Authentication
Authorization
Layered Architecture
Database Relationships
Business Rules
Design Patterns
Caching
Rate Limiting
Realtime Communication
External API Integration
Validation
Error Handling
Concurrency
Security
```

---

# 📌 Final Architecture Summary

```text
                    SALAMTAK

                 Angular Frontend
                        │
                        │
                 REST API + JWT
                        │
                        ▼
              ASP.NET Core Controllers
                        │
                        ▼
                  Service Layer
                        │
             ┌──────────┼──────────┐
             │          │          │
             ▼          ▼          ▼
        Validation    Mapping   Business Rules
             │          │          │
             └──────────┼──────────┘
                        ▼
                  Unit of Work
                        │
                        ▼
               Generic Repository
                        │
                        ▼
                    EF Core
                        │
                        ▼
                  SQL Server


            External / Infrastructure
            ─────────────────────────

                  SignalR
                  Gemini AI
                  Paymob
                  SMTP Email
                  File Storage
                  Output Cache
                  Rate Limiter
```

---

# 👨‍💻 Project Type

**Full-Stack Graduation Project**

The project demonstrates the design and implementation of a real-world healthcare application covering frontend/backend integration, database design, authentication, complex booking workflows, medical data management, real-time communication, external integrations, and scalable backend architecture.

Presnetation For Project   https://canva.link/ks8837zr46934o8 


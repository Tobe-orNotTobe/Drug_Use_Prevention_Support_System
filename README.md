# Drug Use Prevention Support System (DUPSS) 🛡️

**Drug Use Prevention Support System (DUPSS)** is a comprehensive web-based platform built on the **.NET ecosystem**. The project aims to raise awareness, provide educational courses, conduct assessment surveys, and connect users with professional consultants through an online appointment booking system.

---

## 🚀 Key Features

The system implements a robust Role-Based Access Control (RBAC) mechanism with four distinct roles: **Admin, Manager, Consultant, and Member**.

- **👤 Authentication & User Management:**
  - Secure registration and login.
  - Role-based authorization for accessing specific resources.
  - Comprehensive admin/manager dashboard for user lifecycle management.
- **📚 Educational Course System:**
  - Create, update, and manage prevention education courses.
  - Members can browse the catalog, enroll, and track their progress (My Courses).
- **📝 Dynamic Survey & Assessment:**
  - Build dynamic surveys with customizable questions and options.
  - Collect user responses, automatically calculate scores, and generate assessment results.
- **🗓️ Appointment Booking:**
  - Browse verified profiles of prevention consultants.
  - Members can schedule private consultation sessions.
  - Dedicated portals for users (My Appointments) and consultants to manage their schedules.
- **📊 Reports & Dashboard:**
  - Visual statistical charts for system activities.
  - Export capabilities (Excel) for reporting and data analysis.

---

## 🛠️ Tech Stack

- **Backend:** C#, .NET 8 (or your specific version), ASP.NET Core Web API
- **Frontend:** ASP.NET Core MVC (Razor Views), Bootstrap 5, jQuery, CSS/JS
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core (EF Core)
- **Architecture:** 5-Layer N-Tier Architecture, RESTful API

---

## 🏗️ Project Structure (N-Tier Architecture)

The solution is divided into highly decoupled projects following N-Tier principles to ensure scalability and maintainability:

- `BusinessObjects/`: Contains Domain Models, Entities, and Data Transfer Objects (DTOs).
- `DataAccessObjects/ (DAO)`: Interacts directly with the Database context using EF Core.
- `Repositories/`: Abstraction layer wrapping DAOs, implementing interface-driven data access.
- `Services/`: Contains the core Business Logic, acting as the bridge between Repositories and the presentation layer.
- `DUPSWebAPI/`: Exposes RESTful APIs for potential mobile or third-party integrations.
- `DUPSWebApp/`: The main ASP.NET Core MVC application consuming services and rendering the UI.

---

## ⚙️ Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- SQL Server
- Visual Studio / JetBrains Rider / VS Code

### Installation

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/drug_use_prevention_support_system.git](https://github.com/your-username/drug_use_prevention_support_system.git)

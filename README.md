# 🌾 Al-Salem Farm & Inventory Management System

A comprehensive, full-stack **Farm, Invoice, and Inventory Management System** built to streamline daily operations for businesses, tailored initially for agricultural and retail management (مزرعة آل سالم).

This system provides a robust solution for tracking warehouse stock, generating printable invoices, managing employees, and overseeing relationships with customers and suppliers.

---

## ✨ Features

- **📦 Inventory & Store Management:**
  - Track products, categories, and multiple store locations.
  - Monitor stock levels and product transactions in real-time.
- **🧾 Dynamic Invoicing System:**
  - Create and manage sales and purchasing workflows.
  - Generate clean, printable invoices for customers.
- **👥 Stakeholder Management:**
  - Dedicated modules to manage **Customers**, **Suppliers**, and **Employees**.
- **💳 Financial & Transaction Tracking:**
  - Support for multiple payment methods.
  - Detailed transaction reporting for financial audits.
- **📧 Automated Email Notifications:**
  - Built-in SMTP integration for sending system alerts and communications.

---

## 🏗️ Architecture & Tech Stack

This project follows a clean **N-Tier Architecture** to ensure scalability and maintainability:
- **Presentation Layer (Web):** ASP.NET Core MVC with HTML/CSS/JS for a responsive UI.
- **Business Logic Layer (BLL):** Contains core business rules, services, and view models.
- **Data Access Layer (DAL):** Entity Framework Core (EF Core) acting as the ORM, implementing the Repository pattern.
- **Database:** Microsoft SQL Server.

---

## 🚀 Getting Started

Follow these steps to set up the project locally on your machine.

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or your target .NET version)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Abdelrhman-Haroun/System_Managment.git
   cd System_Managment
   ```

2. **Configure your environment:**
   - Navigate to the web project folder: `cd System_Managment`
   - Copy the template configuration file:
     ```bash
     cp appsettings.example.json appsettings.json
     ```
   - Open `appsettings.json` and insert your actual **SQL Server Database Connection String** and **SMTP Email Credentials**.

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Apply Database Migrations:**
   Run Entity Framework Core migrations to create the database schema:
   ```bash
   dotnet ef database update --project ../DAL --startup-project .
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. Open your browser and navigate to `http://localhost:5000` (or the port specified in your console).

---

## 🔐 Security Note

Sensitive configuration files (like `appsettings.json`, `appsettings.Development.json`, and `.env`) are **ignored** in this repository via `.gitignore` to protect credentials. Always use `appsettings.example.json` as a baseline for local configuration.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://github.com/Abdelrhman-Haroun/System_Managment/issues).

## 📄 License

This project is open-source and available under the [MIT License](LICENSE).

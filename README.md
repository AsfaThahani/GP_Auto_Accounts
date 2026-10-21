# GP AutoAccounts | Accounting & Inventory System

GP AutoAccounts is a tailored C# Windows Forms solution designed to automate end-to-end financial and inventory management for GP Automotive Workshop. By analyzing their daily workflows, this system replaces manual tracking with a digital ecosystem powered by SQL Server.

## Tech Stack

- **Language & Framework:** C# (Windows Forms)
- **Database:** SQL Server
- **IDE:** Visual Studio

## Core Features

- **Digital Dashboard:** Tracks real-time sales, expenses, and overall account balances.
- **Automated Financials:** Streamlines invoice generation and financial reporting.
- **Dynamic Client Tracking:** Features dynamic customer balance tracking to easily manage pending payments and credit.
- **Relational Database Modules:** Uses a normalized SQL Server architecture to securely manage workshop inventory and financial transactions.

## Screenshots

**Login Screen**
![Dashboard View](ss/LoginScreen.png)

**Invoice Generation Screen**
![Inventory View](ss/InvoiceCreation.png)

## Getting Started

Follow these instructions to set up and run the project locally on your machine.

### Prerequisites

- Visual Studio (with .NET desktop development workload installed)
- SQL Server (Express, Developer, or Standard edition)

### Installation & Setup

1. **Clone the repository:**
   git clone https://github.com/AsfaThahani/GP_Auto_Accounts.git

2. **Database Setup:**
   * Open SQL Server Management Studio (SSMS).
   * Execute the provided database script (e.g., `schema.sql`) to generate the tables and relations.

3. **Configuration:**
   * Open the project in Visual Studio.
   * Update the connection string in your `App.config` file to match your local SQL Server instance name and credentials.

### Running the Application

1. Open the solution (`.sln`) file in Visual Studio.
2. Build the solution (Ctrl + Shift + B).
3. Press F5 or click Start to run the application.

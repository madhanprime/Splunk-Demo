# 📊 Employee Management & Splunk Observability Demo

[![Angular](https://img.shields.io/badge/Angular-21.2-dd0031.svg?logo=angular)](https://angular.dev/)
[![.NET](https://img.shields.io/badge/.NET-9.0-512bd4.svg?logo=dotnet)](https://dotnet.microsoft.com/)
[![Splunk](https://img.shields.io/badge/Splunk-Enterprise-000000.svg?logo=splunk)](https://www.splunk.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ed.svg?logo=docker)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An end-to-end full-stack demo showcasing **Application Performance Monitoring (APM)**, **Structured JSON Logging**, and **Real-Time Observability** with **Splunk Enterprise**.

---

## 🏛️ System Architecture

```text
                     Angular 21 UI (Port 4200)
                               │
                               │ HTTP REST Requests
                               ▼
                      .NET 9 Web API (Port 5000)
                               │
                ┌──────────────┴──────────────┐
                ▼                             ▼
        SQLite Database            Serilog JSON Logger
     (Pre-seeded Employees)     (./logs/employee-app-*.json)
                                              │
                                              ▼
                                   Splunk Enterprise (Port 8000)
                                              │
                        ┌─────────────────────┼─────────────────────┐
                        ▼                     ▼                     ▼
                   SPL Search            Dashboard Studio         Alerts
```

---

## ✨ Features

- **Angular 21 UI**: Modern dark-themed glassmorphism interface built with Angular 21 (zoneless change detection), employee search/filter, CRUD modals, and a live telemetry streamer.
- **.NET 9 Web API**: High-performance REST API with Entity Framework Core SQLite, database seeding, and Swagger UI.
- **Structured JSON Logging**: Serilog middleware capturing request paths, HTTP methods, status codes, execution duration (`ResponseTimeMs`), client IP, and user agents in compact JSON format.
- **Interactive Anomaly Simulator**: One-click UI buttons to simulate real-world incidents:
  - ⏳ **2.5s Latency Delay** (`/api/employees/simulate/slow`)
  - 🔥 **500 Internal Server Error** (`/api/employees/simulate/error500`)
  - 💾 **Database Timeout Exception** (`/api/employees/simulate/db-timeout`)
  - 🔒 **401 Unauthorized** (`/api/employees/simulate/unauthorized`)
- **Ready-to-Use Splunk Setup**: Pre-configured `docker-compose.yml`, `inputs.conf`, and **Dashboard Studio JSON** (`dashboard_studio.json`).

---

## 📁 Repository Structure

```text
├── EmployeeManagement.Api/      # .NET 9 Web API
│   ├── Controllers/             # AuthController & EmployeesController
│   ├── Data/                    # EF Core AppDbContext & Seed Data
│   ├── Middleware/              # Custom Serilog RequestLoggingMiddleware
│   ├── Models/                  # Employee & Auth Data Contracts
│   └── logs/                    # Compact JSON log output directory
├── EmployeeManagement.UI/       # Angular 21 Frontend Application
│   ├── src/app/components/      # EmployeeList & Login Components
│   ├── src/app/services/        # HTTP API Service & Anomaly Triggers
│   └── src/styles.css           # Glassmorphism Design Tokens
├── splunk/                      # Splunk Infrastructure Configuration
│   ├── dashboard_studio.json    # Splunk Dashboard Studio Definition
│   ├── docker-compose.yml       # Splunk Enterprise Container Config
│   └── inputs.conf              # Directory Monitor Configuration
├── SPLUNK_PROJECT_GUIDE.md      # Detailed Step-by-Step Splunk Tutorial
└── README.md                    # Project Documentation
```

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20+ recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

### 1. Run the .NET 9 Backend
```bash
cd EmployeeManagement.Api
dotnet run --urls "http://localhost:5000"
```
- API Base URL: `http://localhost:5000`
- Swagger Documentation: `http://localhost:5000/swagger`

---

### 2. Run the Angular Frontend
```bash
cd EmployeeManagement.UI
npm install
npx ng serve --port 4200 --open
```
- Open your browser to `http://localhost:4200`
- Demo Login Credentials:
  - **Username**: `admin`
  - **Password**: `password123`

---

### 3. Spin Up Splunk Enterprise via Docker
```bash
cd splunk
docker-compose up -d
```
- Splunk Web Interface: **[http://localhost:8000](http://localhost:8000)**
- **Username**: `admin`
- **Password**: `SplunkPassword123!`

---

## 📊 Splunk Configuration

### 1. Create the Index
1. In Splunk Web, navigate to **Settings** → **Indexes**.
2. Click **New Index**.
3. Name it: **`employee-app`** and click **Save**.

### 2. Ingest Application Logs
1. Navigate to **Settings** → **Data Inputs** → **Files & Directories**.
2. Click **+ Add New** and set the path to:
   ```text
   /opt/splunk/logs
   ```
3. Set **Source Type** to `_json` and select the **`employee-app`** index.
4. Click **Review** → **Submit**.

### 3. Import the Dashboard
1. Go to **Apps** → **Search & Reporting** → **Dashboards**.
2. Click **Create New Dashboard** → select **Dashboard Studio**.
3. Title it **`Employee App Monitoring`**.
4. Open the Source editor (`definition.json`), replace its contents with [`splunk/dashboard_studio.json`](./splunk/dashboard_studio.json), and click **Save**.

---

## 🔍 Essential SPL Queries

| Objective | SPL Query |
| :--- | :--- |
| **All App Logs** | `index="employee-app"` |
| **Filter 4xx & 5xx Errors** | `index="employee-app" StatusCode>=400` |
| **Request Count by Endpoint** | `index="employee-app" \| stats count BY Endpoint \| sort - count` |
| **API Latency (Avg & P95)** | `index="employee-app" \| stats count, avg(ResponseTimeMs) AS Avg_Latency, p95(ResponseTimeMs) AS P95 BY Endpoint Method` |
| **Traffic vs Errors Over Time** | `index="employee-app" \| timechart span=1m count AS Requests, count(eval(StatusCode>=400)) AS Errors` |
| **Detect Slow APIs (> 500ms)** | `index="employee-app" ResponseTimeMs>500 \| table _time Endpoint Method ResponseTimeMs ClientIp` |
| **Brute Force Login Detection** | `index="employee-app" EventType="UserLoginFailure" \| stats count BY Username ClientIp \| where count >= 3` |

---

## 📄 License
This project is open-source under the [MIT License](LICENSE).

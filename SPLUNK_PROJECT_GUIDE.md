# 🚀 Employee Management API + Splunk Monitoring Demo Guide

Welcome to your hands-on **Splunk Monitoring Demo Project**! This project guides you step-by-step from building a **.NET Web API** with **structured JSON logging** to ingesting logs into **Splunk**, writing **SPL queries**, creating a **monitoring dashboard**, and setting up **alerts**.

---

## 🛠 Project Structure

```
c:\Users\USER\Documents\splunk\
├── EmployeeManagement.Api/          # .NET Web API with Serilog JSON Logging
│   ├── Controllers/
│   │   ├── AuthController.cs        # Login endpoint (logs 200 / 401 attempts)
│   │   └── EmployeesController.cs   # Employee CRUD + Anomaly simulation routes
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core SQLite DbContext with seed data
│   ├── Middleware/
│   │   └── RequestLoggingMiddleware.cs # Custom HTTP telemetry middleware
│   ├── Models/
│   │   └── Employee.cs              # Data contracts
│   └── logs/                        # Generated compact JSON logs (Splunk target)
└── splunk/                          # Splunk docker & configuration
    ├── docker-compose.yml           # Runs Splunk Enterprise container
    └── inputs.conf                  # Splunk directory monitor configuration
```

---

## 🚦 Stage 1 & 2: .NET API & Structured Logging (Completed)

The **.NET Web API** is configured with **Serilog** to output compact JSON logs into `./logs/employee-app-yyyyMMdd.json`.

### 📄 Sample Log Record
```json
{
  "@t": "2026-09-28T13:20:00.1234567Z",
  "@mt": "HTTP {Method} {Endpoint} responded {StatusCode} in {ResponseTimeMs}ms",
  "Application": "EmployeeApp",
  "Endpoint": "/api/employees",
  "Method": "GET",
  "StatusCode": 200,
  "ResponseTimeMs": 42,
  "ClientIp": "127.0.0.1",
  "UserAgent": "Mozilla/5.0..."
}
```

---

## 📥 Stage 3: Ingesting Logs into Splunk

### Option A: Using Docker (Recommended)
1. Run Splunk via Docker Compose:
   ```bash
   cd c:\Users\USER\Documents\splunk\splunk
   docker-compose up -d
   ```
2. Access Splunk Web UI at `http://localhost:8000` (User: `admin` | Password: `SplunkPassword123!`).
3. Create Index `employee-app`:
   - Go to **Settings** -> **Indexes** -> **New Index**.
   - Index Name: `employee-app`.

### Option B: Local File Monitor Input
Add the log path under **Settings** -> **Data Inputs** -> **Files & Directories**:
- Path: `c:\Users\USER\Documents\splunk\EmployeeManagement.Api\logs\`
- Index: `employee-app`
- Source type: `_json`

---

## 🔍 Stage 4: Learn SPL (Splunk Processing Language)

Here is your step-by-step SPL learning path from basic search to advanced analytics:

### 1. Basic Search
```spl
index="employee-app"
```

### 2. Filter Errors (4xx & 5xx)
```spl
index="employee-app" (StatusCode>=400 OR Level="Error")
```

### 3. Count Total Requests
```spl
index="employee-app" | stats count AS Total_Requests
```

### 4. Breakdown Requests by Endpoint
```spl
index="employee-app" | stats count BY Endpoint | sort - count
```

### 5. API Response Time Performance (Avg, P95, Max)
```spl
index="employee-app" 
| stats count, avg(ResponseTimeMs) AS Avg_Latency_ms, p95(ResponseTimeMs) AS P95_Latency_ms, max(ResponseTimeMs) AS Max_Latency_ms BY Endpoint Method 
| eval Avg_Latency_ms=round(Avg_Latency_ms, 2)
```

### 6. Timechart of Requests & Errors over Time
```spl
index="employee-app" 
| timechart span=1m count AS Total_Requests, count(eval(StatusCode>=400)) AS Error_Count
```

### 7. Slow APIs Detection (> 500ms)
```spl
index="employee-app" ResponseTimeMs>500 
| table _time Endpoint Method ResponseTimeMs StatusCode ClientIp
```

### 8. Failed Login Attempts Monitor
```spl
index="employee-app" EventType="UserLoginFailure" 
| stats count BY Username ClientIp 
| where count >= 3
```

---

## 📊 Stage 5: Splunk Dashboard

Splunk supports two dashboard types: **Dashboard Studio (JSON)** and **Classic Dashboards (XML)**.

### Option A: Dashboard Studio (JSON) - Recommended
When creating a dashboard using **Dashboard Studio**, open the source code (`definition.json`) and paste this JSON:

```json
{
  "visualizations": {
    "viz_total_requests": {
      "type": "splunk.singlevalue",
      "dataSources": { "primary": "ds_total_requests" },
      "title": "Total Requests",
      "options": { "majorColor": "#38bdf8" }
    },
    "viz_total_errors": {
      "type": "splunk.singlevalue",
      "dataSources": { "primary": "ds_total_errors" },
      "title": "Total Errors (4xx / 5xx)",
      "options": { "majorColor": "#ef4444" }
    },
    "viz_avg_latency": {
      "type": "splunk.singlevalue",
      "dataSources": { "primary": "ds_avg_latency" },
      "title": "Avg Response Time (ms)",
      "options": { "majorColor": "#10b981", "unit": "ms" }
    },
    "viz_traffic_chart": {
      "type": "splunk.area",
      "dataSources": { "primary": "ds_traffic_chart" },
      "title": "Traffic Volume & Errors Over Time"
    },
    "viz_endpoint_table": {
      "type": "splunk.table",
      "dataSources": { "primary": "ds_endpoint_table" },
      "title": "Endpoint Response Performance"
    }
  },
  "dataSources": {
    "ds_total_requests": {
      "type": "ds.search",
      "options": { "query": "index=\"employee-app\" | stats count" }
    },
    "ds_total_errors": {
      "type": "ds.search",
      "options": { "query": "index=\"employee-app\" StatusCode>=400 | stats count" }
    },
    "ds_avg_latency": {
      "type": "ds.search",
      "options": { "query": "index=\"employee-app\" | stats avg(ResponseTimeMs) as avg_latency | eval avg_latency=round(avg_latency,1)" }
    },
    "ds_traffic_chart": {
      "type": "ds.search",
      "options": { "query": "index=\"employee-app\" | timechart span=5m count as Requests, count(eval(StatusCode>=400)) as Errors" }
    },
    "ds_endpoint_table": {
      "type": "ds.search",
      "options": { "query": "index=\"employee-app\" | stats count, avg(ResponseTimeMs) as Avg_ms, max(ResponseTimeMs) as Max_ms by Method Endpoint | eval Avg_ms=round(Avg_ms, 1)" }
    }
  },
  "defaults": {
    "dataSources": {
      "ds.search": {
        "options": { "queryParameters": { "earliest": "-24h@h", "latest": "now" } }
      }
    }
  },
  "layout": {
    "type": "absolute",
    "options": { "width": 1240, "height": 900, "display": "auto-scale" },
    "structure": [
      { "item": "viz_total_requests", "position": { "x": 20, "y": 20, "w": 380, "h": 150 } },
      { "item": "viz_total_errors", "position": { "x": 420, "y": 20, "w": 380, "h": 150 } },
      { "item": "viz_avg_latency", "position": { "x": 820, "y": 20, "w": 380, "h": 150 } },
      { "item": "viz_traffic_chart", "position": { "x": 20, "y": 190, "w": 1180, "h": 320 } },
      { "item": "viz_endpoint_table", "position": { "x": 20, "y": 530, "w": 1180, "h": 340 } }
    ]
  },
  "title": "Employee App Monitoring Dashboard"
}
```

---

### Option B: Classic XML Dashboard
If you created a **Classic Dashboard**:

```xml
<dashboard version="1.1" theme="dark">
  <label>Employee App Monitoring Dashboard</label>
  <row>
    <panel>
      <single>
        <title>Total Requests</title>
        <search>
          <query>index="employee-app" | stats count</query>
          <earliest>-24h@h</earliest>
          <latest>now</latest>
        </search>
      </single>
    </panel>
    <panel>
      <single>
        <title>Total Errors (4xx / 5xx)</title>
        <search>
          <query>index="employee-app" StatusCode>=400 | stats count</query>
          <earliest>-24h@h</earliest>
          <latest>now</latest>
        </search>
      </single>
    </panel>
    <panel>
      <single>
        <title>Avg Latency (ms)</title>
        <search>
          <query>index="employee-app" | stats avg(ResponseTimeMs) as avg_latency | eval avg_latency=round(avg_latency,1)</query>
          <earliest>-24h@h</earliest>
          <latest>now</latest>
        </search>
      </single>
    </panel>
  </row>
  <row>
    <panel>
      <chart>
        <title>Traffic Volume &amp; Errors Over Time</title>
        <search>
          <query>index="employee-app" | timechart span=5m count as Requests, count(eval(StatusCode>=400)) as Errors</query>
        </search>
        <option name="charting.chart">line</option>
      </chart>
    </panel>
    <panel>
      <table>
        <title>Endpoint Response Performance</title>
        <search>
          <query>index="employee-app" | stats count, avg(ResponseTimeMs) as Avg_ms, max(ResponseTimeMs) as Max_ms by Method Endpoint | eval Avg_ms=round(Avg_ms, 1)</query>
        </search>
      </table>
    </panel>
  </row>
</dashboard>
```

---

## 🚨 Stage 6: Setting Up Alerts

### Alert Rule: High 500 Internal Server Errors
- **SPL Query**:
  ```spl
  index="employee-app" StatusCode>=500
  ```
- **Time Window**: Last 5 minutes (`-5m`)
- **Trigger Condition**: Number of Results `> 5`
- **Actions**: Trigger Log Event / Send Email Alert / Webhook notification.

---

## 🧪 Testing Telemetry Anomaly Endpoints

Use these built-in simulation endpoints to test Splunk dashboards and alerts:

1. **Simulate Slow Response**: `GET http://localhost:5000/api/employees/simulate/slow` (Adds 2500ms latency)
2. **Simulate 500 Error**: `GET http://localhost:5000/api/employees/simulate/error500` (Triggers DB connection exception)
3. **Simulate DB Timeout**: `GET http://localhost:5000/api/employees/simulate/db-timeout` (Returns 500 DB timeout)
4. **Simulate Unauthorized**: `GET http://localhost:5000/api/employees/simulate/unauthorized` (Returns 401 Unauthorized)

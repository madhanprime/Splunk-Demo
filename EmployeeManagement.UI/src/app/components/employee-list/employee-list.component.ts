import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../../services/employee.service';
import { Employee, TelemetryLog } from '../../models/employee.model';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.css']
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  searchTerm = '';
  selectedDepartment = '';
  isLoading = false;
  
  // Modal State
  showModal = false;
  isEditing = false;
  currentEmployee: Employee = {
    firstName: '',
    lastName: '',
    email: '',
    department: 'Engineering',
    salary: 80000,
    role: 'Software Engineer'
  };

  // Telemetry Log Feed
  telemetryLogs: TelemetryLog[] = [];

  constructor(
    private employeeService: EmployeeService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading = true;
    const startTime = performance.now();

    this.employeeService.getEmployees(this.searchTerm, this.selectedDepartment).subscribe({
      next: (data) => {
        const latency = Math.round(performance.now() - startTime);
        this.employees = data;
        this.isLoading = false;
        this.addTelemetryLog('GET /api/employees', 200, `Retrieved ${data.length} records`, latency);
        this.cdr.markForCheck();
      },
      error: (err) => {
        const latency = Math.round(performance.now() - startTime);
        this.isLoading = false;
        this.addTelemetryLog('GET /api/employees', err.status || 500, err.message, latency);
        this.cdr.markForCheck();
      }
    });
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.currentEmployee = {
      firstName: '',
      lastName: '',
      email: '',
      department: 'Engineering',
      salary: 85000,
      role: 'Developer'
    };
    this.showModal = true;
  }

  openEditModal(emp: Employee): void {
    this.isEditing = true;
    this.currentEmployee = { ...emp };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  saveEmployee(): void {
    const startTime = performance.now();
    if (this.isEditing && this.currentEmployee.id) {
      this.employeeService.updateEmployee(this.currentEmployee.id, this.currentEmployee).subscribe({
        next: () => {
          const latency = Math.round(performance.now() - startTime);
          this.addTelemetryLog(`PUT /api/employees/${this.currentEmployee.id}`, 200, 'Employee updated', latency);
          this.closeModal();
          this.loadEmployees();
        },
        error: (err) => {
          const latency = Math.round(performance.now() - startTime);
          this.addTelemetryLog(`PUT /api/employees/${this.currentEmployee.id}`, err.status || 500, 'Update failed', latency);
        }
      });
    } else {
      this.employeeService.createEmployee(this.currentEmployee).subscribe({
        next: (created) => {
          const latency = Math.round(performance.now() - startTime);
          this.addTelemetryLog('POST /api/employees', 201, `Created employee ID ${created.id}`, latency);
          this.closeModal();
          this.loadEmployees();
        },
        error: (err) => {
          const latency = Math.round(performance.now() - startTime);
          this.addTelemetryLog('POST /api/employees', err.status || 400, 'Validation failed', latency);
        }
      });
    }
  }

  deleteEmployee(id?: number): void {
    if (!id || !confirm('Are you sure you want to delete this employee record?')) return;
    const startTime = performance.now();
    this.employeeService.deleteEmployee(id).subscribe({
      next: () => {
        const latency = Math.round(performance.now() - startTime);
        this.addTelemetryLog(`DELETE /api/employees/${id}`, 200, 'Record deleted', latency);
        this.loadEmployees();
      },
      error: (err) => {
        const latency = Math.round(performance.now() - startTime);
        this.addTelemetryLog(`DELETE /api/employees/${id}`, err.status || 500, 'Delete failed', latency);
      }
    });
  }

  // Fire Anomaly Telemetry Event directly to .NET -> Splunk
  triggerAnomaly(type: string): void {
    const startTime = performance.now();
    this.employeeService.simulateAnomaly(type).subscribe({
      next: (res) => {
        const latency = Math.round(performance.now() - startTime);
        this.addTelemetryLog(`GET /api/employees/simulate/${type}`, 200, res.message || 'Anomaly executed', latency);
      },
      error: (err) => {
        const latency = Math.round(performance.now() - startTime);
        const status = err.status || 500;
        this.addTelemetryLog(`GET /api/employees/simulate/${type}`, status, `Anomaly triggered HTTP ${status}`, latency);
      }
    });
  }

  private addTelemetryLog(type: string, status: number, message: string, latencyMs: number): void {
    const newLog: TelemetryLog = {
      time: new Date().toLocaleTimeString(),
      type,
      status,
      message,
      latencyMs
    };
    this.telemetryLogs.unshift(newLog);
    if (this.telemetryLogs.length > 8) {
      this.telemetryLogs.pop();
    }
    this.cdr.markForCheck();
  }
}

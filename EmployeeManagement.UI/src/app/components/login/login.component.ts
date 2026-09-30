import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EmployeeService } from '../../services/employee.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username = 'admin';
  password = 'password123';
  errorMessage = '';
  successMessage = '';
  isLoading = false;

  constructor(
    private employeeService: EmployeeService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  onLogin(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.employeeService.login({ username: this.username, password: this.password }).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          localStorage.setItem('auth_token', res.token);
          this.successMessage = `Login Successful! Event logged to Splunk index: employee-app`;
          this.cdr.markForCheck();
          setTimeout(() => this.router.navigate(['/employees']), 1200);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = `Login Failed (401 Unauthorized). Telemetry logged to Splunk!`;
        this.cdr.markForCheck();
      }
    });
  }
}

export interface Employee {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  salary: number;
  role: string;
  createdAt?: string;
}

export interface LoginRequest {
  username?: string;
  password?: string;
}

export interface LoginResponse {
  success: boolean;
  username: string;
  token: string;
  message: string;
}

export interface TelemetryLog {
  time: string;
  type: string;
  status: number;
  message: string;
  latencyMs: number;
}

# ========================================================
# Full-Stack Deployment Dockerfile for Render / Cloud
# Builds Angular 21 UI + .NET 9 API into a single container
# ========================================================

# Stage 1: Build Angular Frontend
FROM node:22-alpine AS angular-build
WORKDIR /app/ui

# Install dependencies
COPY EmployeeManagement.UI/package*.json ./
RUN npm ci

# Copy frontend source and build production bundle
COPY EmployeeManagement.UI/ ./
RUN npx ng build --configuration production

# Stage 2: Build .NET 9 Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /app/api

# Restore dependencies
COPY EmployeeManagement.Api/*.csproj ./
RUN dotnet restore

# Copy backend source and publish
COPY EmployeeManagement.Api/ ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Copy built Angular static assets into ASP.NET Core wwwroot
COPY --from=angular-build /app/ui/dist/EmployeeManagement.UI/browser /app/publish/wwwroot

# Stage 3: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install curl for Render health checks and timezone data
RUN apk add --no-cache curl tzdata

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application from build stage
COPY --from=dotnet-build /app/publish ./

# Render dynamically passes $PORT (default 10000)
ENV PORT=10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

# Start application binding to dynamic $PORT
ENTRYPOINT ["sh", "-c", "exec dotnet EmployeeManagement.Api.dll --urls http://0.0.0.0:${PORT}"]

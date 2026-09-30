# ========================================================
# .NET 9 Web API Dockerfile for Render / Cloud Deployment
# ========================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache curl tzdata
RUN mkdir -p /app/logs

COPY --from=build /app/publish ./

ENV PORT=10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "exec dotnet EmployeeManagement.Api.dll --urls http://0.0.0.0:${PORT}"]

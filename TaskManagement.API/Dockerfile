# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManagement.Domain/TaskManagement.Domain.csproj TaskManagement.Domain/
COPY TaskManagement.Application/TaskManagement.Application.csproj TaskManagement.Application/
COPY TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj TaskManagement.Infrastructure/
COPY TaskManagement.API/TaskManagement.API.csproj TaskManagement.API/
RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj

COPY . .
RUN dotnet publish TaskManagement.API/TaskManagement.API.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]

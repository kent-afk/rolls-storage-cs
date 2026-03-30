FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/API/API.csproj", "API/"]
COPY ["src/Core/Core.csproj", "Core/"]
COPY ["src/Entity/Entity.csproj", "Entity/"]
COPY ["src/Services/Services.csproj", "Services/"]

# Restore dependencies
RUN dotnet restore API/API.csproj

# Copy all source files
COPY src/ ./src/

# Build the application
WORKDIR /src/API
RUN dotnet build API.csproj -c Release -o /app/build
RUN dotnet publish API.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 5000
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "API.dll"]

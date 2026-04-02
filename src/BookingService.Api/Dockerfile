# Use a base image with .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build AS builder
WORKDIR /app

# Copy csproj files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Use a smaller runtime image for the final application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output from the builder stage
COPY --from=builder /app/out .

# Expose the port the application listens on (default for Kestrel is 80)
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "BookingService.Api.dll"]

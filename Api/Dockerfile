# ใช้ base image สำหรับ runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# สร้าง non-root user สำหรับความปลอดภัย
RUN addgroup --system --gid 1001 dotnetgroup \
    && adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser

# ใช้ SDK image สำหรับ build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file และ restore dependencies (สำหรับ Docker layer caching)
COPY ["Api.csproj", "./"]
RUN dotnet restore "Api.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app

# Install curl สำหรับ health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# เปลี่ยน ownership ของไฟล์ให้ non-root user
RUN chown -R dotnetuser:dotnetgroup /app

# Switch ไปใช้ non-root user
USER dotnetuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Api.dll"]